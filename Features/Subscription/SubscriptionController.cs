using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IO;
using listly.Features.Firebase;
using listly.Features.User;

namespace listly.Features.Subscription
{
  [Route("api/subscriptions")]
  [ApiController]
  [Authorize]
  public class SubscriptionController : ControllerBase
  {
    private readonly SubscriptionService _service;
    private readonly FirebaseService? _firebaseService;
    private readonly UserServices? _userServices;

    public SubscriptionController(SubscriptionService service, FirebaseService? firebaseService = null, UserServices? userServices = null)
    {
      _service = service;
      _firebaseService = firebaseService;
      _userServices = userServices;
    }

    // GET: api/subscriptions
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SubscriptionDto>>> GetSubscriptions()
    {
      var subscriptions = await _service.GetAllAsync();
      return Ok(subscriptions);
    }

    // POST: api/subscriptions/monthly
    [HttpPost("monthly")]
    public async Task<IActionResult> CreateMonthlySubscription()
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

      if (string.IsNullOrEmpty(userId))
      {
        return Unauthorized("Usuario no autenticado correctamente");
      }

      if (_userServices == null)
      {
        return StatusCode(500, "Servicio de usuario no disponible");
      }

      var user = await _userServices.GetUserByUIdAsync(userId);
      var userEmail = user?.Email;

      if (string.IsNullOrEmpty(userEmail))
      {
        return Unauthorized("No se pudo obtener el email del usuario");
      }

      try
      {
        var subscriptionResponse = await _service.CreateMonthlySubscriptionAsync(userId, userEmail);
        return Ok(subscriptionResponse);
      }
      catch (KeyNotFoundException ex)
      {
        return NotFound(ex.Message);
      }
      catch (InvalidOperationException ex)
      {
        return BadRequest(ex.Message);
      }
      catch (Exception ex)
      {
        return StatusCode(500, $"Error interno del servidor: {ex.Message}");
      }
    }

    // POST: api/subscriptions/webhook
    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> ProcessPaymentWebhook()
    {
      try
      {
        // Leer el body de la petición del webhook
        using var reader = new StreamReader(Request.Body);
        var jsonBody = await reader.ReadToEndAsync();

        // Parsear la notificación de MercadoPago
        var notification = System.Text.Json.JsonSerializer.Deserialize<MercadoPagoWebhookDto>(jsonBody);

        if (notification == null)
        {
          return BadRequest("Invalid webhook data - notification is null");
        }

        string? paymentId = null;
        string? directExternalReference = notification.external_reference;

        // Determinar el tipo de notificación y extraer el payment ID correspondiente
        if (notification.topic == "merchant_order")
        {
          // Para merchant orders, extraer ID de la URL del resource
          var merchantOrderId = notification.GetMerchantOrderId();
          if (!string.IsNullOrEmpty(merchantOrderId))
          {
            paymentId = merchantOrderId;
          }
        }
        else
        {
          // Para payment o subscription_preapproval, usar el ID de data o principal
          paymentId = notification.data?.Id ?? notification.GetIdAsString();
        }

        if (string.IsNullOrEmpty(paymentId))
        {
          return BadRequest("Invalid webhook data - no payment ID found");
        }

        // Caso especial: subscription_preapproval sin external_reference
        if (notification.type == "subscription_preapproval" && string.IsNullOrEmpty(directExternalReference))
        {
          return Ok(new { received = true, success = true, message = "Subscription preapproval received but not processed (no external_reference)" });
        }

        // Procesar la notificación de pago a través del servicio
        var result = await _service.ProcessPaymentNotificationAsync(paymentId, directExternalReference);

        if (result.Success)
        {
          // Enviar notificación push al usuario si tiene FCM token válido configurado
          // if (!string.IsNullOrEmpty(result.UserFcmToken) && result.SubscriptionActivated && _firebaseService != null)
          // {
          //   try
          //   {
          //     await _firebaseService.SendNotificationAsync(new Firebase.NotificationRequest
          //     {
          //       FcmToken = result.UserFcmToken,
          //       Title = "🎉 ¡Suscripción activada!",
          //       Body = "Tu suscripción mensual ha sido activada exitosamente.",
          //       ClickAction = "OPEN_SUBSCRIPTION",
          //       Data = new Dictionary<string, string>
          //       {
          //         ["type"] = "profile",
          //         // ["subscriptionId"] = result.SubscriptionId.ToString()
          //       }
          //     });

          //     //  ["type"] = "subscription_activated",
          //     // "title": "Pago exitoso",
          //     // "body": "Tu suscripción premium fue activada.",
          //     // "type": "profile"

          //   }
          //   catch (Exception notificationEx)
          //   {
          //     // Log del error pero no fallar el webhook
          //     Console.WriteLine($"Error enviando notificación push: {notificationEx.Message}");
          //   }
          // }

          return Ok(new { received = true, success = true, message = "Subscription activated successfully" });
        }
        else
        {
          return BadRequest(new { error = result.Message });
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"WEBHOOK ERROR: {ex.Message}");
        return StatusCode(500, new { error = "Internal server error" });
      }
    }
  }
}
