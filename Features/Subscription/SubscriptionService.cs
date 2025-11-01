using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json;
using listly.Features.User;

namespace listly.Features.Subscription
{
  public class SubscriptionService
  {
    private readonly ListlyDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public SubscriptionService(ListlyDbContext context, IConfiguration configuration)
    {
      _context = context;
      _configuration = configuration;
      _httpClient = new HttpClient();
    }

    // Obtener todas las suscripciones disponibles
    public async Task<List<SubscriptionDto>> GetAllAsync()
    {
      return await _context.Set<Subscription>()
          .Select(s => new SubscriptionDto
          {
            SubscriptionId = s.SubscriptionId,
            Name = s.Name,
            Description = s.Description,
            Price = s.Price
          })
          .ToListAsync();
    }

    // Obtener una suscripción por ID
    public async Task<SubscriptionDto?> GetByIdAsync(int id)
    {
      var subscription = await _context.Set<Subscription>()
          .FirstOrDefaultAsync(s => s.SubscriptionId == id);

      if (subscription == null)
        return null;

      return new SubscriptionDto
      {
        SubscriptionId = subscription.SubscriptionId,
        Name = subscription.Name,
        Description = subscription.Description,
        Price = subscription.Price
      };
    }

    // Crear una suscripción mensual usando MercadoPago
    public async Task<SubscriptionResponseDto> CreateMonthlySubscriptionAsync(string userId, string userEmail)
    {
      // Obtener la suscripción Premium con ID = 2 desde la base de datos
      var subscription = await GetByIdAsync(2);
      if (subscription == null)
      {
        throw new KeyNotFoundException("Suscripción Premium con ID 2 no encontrada");
      }

      // Verificar configuración de MercadoPago
      var accessToken = _configuration["MercadoPago:AccessToken"];
      if (string.IsNullOrEmpty(accessToken))
      {
        throw new InvalidOperationException("AccessToken de MercadoPago no configurado");
      }

      // Crear preferencia de pago usando la API REST de MercadoPago
      var preferenceData = new
      {
        items = new[]
          {
            new
            {
              title = $"{subscription.Name} - Suscripción Mensual",
              description = subscription.Description,
              quantity = 1,
              currency_id = "ARS",
              unit_price = subscription.Price
            }
          },
        payer = new
        {
          email = userEmail
        },
        back_urls = new
        {
          success = "lsq://pago/exitoso",
          failure = "lsq://pago/error",
          pending = "lsq://pago/pendiente"
        },
        auto_return = "approved",
        external_reference = $"subscription_{userId}_{DateTime.UtcNow:yyyyMMddHHmmss}",
        notification_url = _configuration["APIURL:BaseURL"] + "/api/subscriptions/webhook",
        expires = false
      };

      // Configurar autenticación para la API de MercadoPago
      _httpClient.DefaultRequestHeaders.Clear();
      _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

      // Enviar request para crear preferencia
      var response = await _httpClient.PostAsJsonAsync(
          "https://api.mercadopago.com/checkout/preferences",
          preferenceData
      );

      if (!response.IsSuccessStatusCode)
      {
        var errorContent = await response.Content.ReadAsStringAsync();
        throw new InvalidOperationException($"Error creando preferencia en MercadoPago: {errorContent}");
      }

      // Leer el contenido de la respuesta
      var responseContent = await response.Content.ReadAsStringAsync();
      // Analizar la respuesta JSON
      var preferenceResponse = await response.Content.ReadFromJsonAsync<MercadoPagoPreferenceResponse>();

      // GUARDAR MAPEO: Preference ID -> User UID para identificar el usuario en el webhook
      var externalRef = $"subscription_{userId}_{DateTime.UtcNow:yyyyMMddHHmmss}";
      var paymentMapping = new PaymentMapping
      {
        UserUid = userId,
        PreferenceId = preferenceResponse?.Id ?? "",
        ExternalReference = externalRef
      };

      _context.PaymentMappings.Add(paymentMapping);
      await _context.SaveChangesAsync();

      // Retornar DTO con los datos de la preferencia para el frontend
      return new SubscriptionResponseDto
      {
        SubscriptionId = subscription.SubscriptionId,
        Name = subscription.Name,
        Description = subscription.Description,
        Price = subscription.Price,
        MercadoPagoPreferenceId = preferenceResponse?.Id,
        InitPoint = preferenceResponse?.GetInitPoint(),
        SandboxInitPoint = preferenceResponse?.GetSandboxInitPoint()
      };
    }

    // Procesar notificación de pago desde MercadoPago
    public async Task<PaymentProcessingResult> ProcessPaymentNotificationAsync(string paymentId, string? directExternalReference = null)
    {
      try
      {
        // Verificar configuración del Access Token
        var accessToken = _configuration["MercadoPago:AccessToken"];
        if (string.IsNullOrEmpty(accessToken))
        {
          return new PaymentProcessingResult { Success = false, Message = "AccessToken no configurado" };
        }

        // Configurar headers para API de MercadoPago
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

        string? externalReference = directExternalReference;

        // Si no tenemos external_reference directo, intentar obtenerlo de MercadoPago
        if (string.IsNullOrEmpty(externalReference))
        {
          // Intentar consultar como Payment en la API de MercadoPago
          var paymentResponse = await _httpClient.GetAsync($"https://api.mercadopago.com/v1/payments/{paymentId}");

          if (paymentResponse.IsSuccessStatusCode)
          {
            // Es un payment válido, verificar que esté aprobado
            var paymentData = await paymentResponse.Content.ReadFromJsonAsync<MercadoPagoPaymentResponse>();

            if (paymentData?.Status != "approved")
            {
              return new PaymentProcessingResult { Success = false, Message = $"Pago no aprobado. Estado: {paymentData?.Status}" };
            }

            externalReference = paymentData.ExternalReference;
          }
          else
          {
            // Si no es payment, intentar como Merchant Order
            var merchantOrderResponse = await _httpClient.GetAsync($"https://api.mercadolibre.com/merchant_orders/{paymentId}");

            if (merchantOrderResponse.IsSuccessStatusCode)
            {
              var merchantOrder = await merchantOrderResponse.Content.ReadFromJsonAsync<MercadoPagoMerchantOrderResponse>();
              externalReference = merchantOrder?.ExternalReference;

              // Verificar que tenga pagos aprobados
              if (merchantOrder?.Payments == null || !merchantOrder.Payments.Any(p => p.Status == "approved"))
              {
                return new PaymentProcessingResult { Success = false, Message = "Merchant Order sin pagos aprobados" };
              }
            }
            else
            {
              // Si no es Merchant Order, intentar como Preapproval
              var preapprovalResponse = await _httpClient.GetAsync($"https://api.mercadopago.com/preapproval/{paymentId}");

              if (preapprovalResponse.IsSuccessStatusCode)
              {
                // Para preapproval, NO podemos determinar el usuario real sin más información
                // Este caso requiere que el external_reference venga del preapproval
                return new PaymentProcessingResult { Success = false, Message = "Preapproval sin external_reference - no se puede determinar usuario" };
              }
              else
              {
                // Buscar en tabla de mapeo el usuario que hizo el pago más reciente
                var recentMapping = await _context.PaymentMappings
                  .Where(pm => pm.CreatedAt > DateTime.UtcNow.AddMinutes(-10))
                  .OrderByDescending(pm => pm.CreatedAt)
                  .FirstOrDefaultAsync();

                if (recentMapping != null)
                {
                  externalReference = recentMapping.ExternalReference;
                }
                else
                {
                  return new PaymentProcessingResult { Success = false, Message = "No se pudo identificar el usuario del pago" };
                }
              }
            }
          }
        }

        // Validar external_reference (tanto si viene directo como si se obtuvo de MercadoPago)
        if (string.IsNullOrEmpty(externalReference) || !externalReference.StartsWith("subscription_"))
        {
          return new PaymentProcessingResult { Success = false, Message = "Referencia externa inválida" };
        }

        // Extraer userId del external_reference (formato: subscription_{userId}_{timestamp})
        var parts = externalReference.Split('_');
        if (parts.Length < 3)
        {
          return new PaymentProcessingResult { Success = false, Message = "Formato de referencia externa inválido" };
        }

        var userId = parts[1];

        // Verificar que el usuario existe
        var user = await _context.User.FirstOrDefaultAsync(u => u.UId == userId);
        if (user == null)
        {
          return new PaymentProcessingResult { Success = false, Message = "Usuario no encontrado" };
        }

        // Obtener la suscripción Premium (ID = 2)
        var subscription = await _context.Set<Subscription>().FindAsync(2);
        if (subscription == null)
        {
          return new PaymentProcessingResult { Success = false, Message = "Suscripción base no encontrada" };
        }

        // Calcular fechas de la suscripción (válida por 1 mes)
        var now = DateTime.UtcNow;
        var startDate = now;
        var endDate = now.AddMonths(1);

        // Verificar si el usuario ya tiene una suscripción activa
        var existingSubscription = await _context.Set<UserSubscription>()
          .Where(us => us.UId == userId)
          .FirstOrDefaultAsync();

        if (existingSubscription != null)
        {
          // Eliminar suscripción existente y crear nueva Premium
          _context.Set<UserSubscription>().Remove(existingSubscription);
          await _context.SaveChangesAsync();

          // Crear nueva suscripción Premium
          var newUserSubscription = new UserSubscription
          {
            UId = userId,
            SubscriptionId = subscription.SubscriptionId, // Premium (ID=2)
            StartDate = startDate,
            EndDate = endDate
          };

          _context.Set<UserSubscription>().Add(newUserSubscription);
          await _context.SaveChangesAsync();
        }
        else
        {
          // Crear nueva suscripción Premium
          var userSubscription = new UserSubscription
          {
            UId = userId,
            SubscriptionId = subscription.SubscriptionId,
            StartDate = startDate,
            EndDate = endDate
          };

          _context.Set<UserSubscription>().Add(userSubscription);
          await _context.SaveChangesAsync();
        }

        // Retornar resultado exitoso con datos para Firebase notification
        return new PaymentProcessingResult
        {
          Success = true,
          Message = "Suscripción activada exitosamente",
          SubscriptionId = subscription.SubscriptionId,
          UserFcmToken = user.FcmToken,
          SubscriptionActivated = true
        };
      }
      catch (Exception ex)
      {
        return new PaymentProcessingResult
        {
          Success = false,
          Message = $"Error procesando pago: {ex.Message}"
        };
      }
    }
  }

  // DTOs internos para manejar respuestas de MercadoPago (un desastre de naming conventions, pero bue...)
  internal class MercadoPagoPreferenceResponse
  {
    public string? Id { get; set; }
    public string? InitPoint { get; set; }
    public string? SandboxInitPoint { get; set; }

    // Propiedades adicionales que MercadoPago podría devolver
    public string? init_point { get; set; }
    public string? sandbox_init_point { get; set; }

    // Constructor que maneja ambos formatos de naming
    public string? GetInitPoint() => InitPoint ?? init_point;
    public string? GetSandboxInitPoint() => SandboxInitPoint ?? sandbox_init_point;
  }

  internal class MercadoPagoPaymentResponse
  {
    public string? Status { get; set; }
    public string? ExternalReference { get; set; }
  }

  internal class MercadoPagoMerchantOrderResponse
  {
    public string? ExternalReference { get; set; }
    public List<MerchantOrderPayment>? Payments { get; set; }
  }

  internal class MerchantOrderPayment
  {
    public string? Status { get; set; }
    public string? Id { get; set; }
  }
}
