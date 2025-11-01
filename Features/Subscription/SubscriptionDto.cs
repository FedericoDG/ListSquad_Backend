namespace listly.Features.Subscription
{
  public class SubscriptionDto
  {
    public int SubscriptionId { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public decimal Price { get; set; }
  }

  public class SubscriptionResponseDto
  {
    public int SubscriptionId { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public decimal Price { get; set; }
    public string? MercadoPagoPreferenceId { get; set; }
    public string? InitPoint { get; set; }
    public string? SandboxInitPoint { get; set; }
  }

  public class MercadoPagoWebhookDto
  {
    // Formato subscription_preapproval
    public string? action { get; set; }
    public string? application_id { get; set; }
    public WebhookData? data { get; set; }
    public string? date { get; set; }
    public string? entity { get; set; }
    public object? id { get; set; } // Puede ser string o number
    public string? type { get; set; }
    public int? version { get; set; }

    // Formato payment
    public string? api_version { get; set; }
    public string? date_created { get; set; }
    public bool? live_mode { get; set; }
    public object? user_id { get; set; } // Puede ser string o number
    public string? external_reference { get; set; }

    // Formato merchant_order
    public string? resource { get; set; }
    public string? topic { get; set; }

    // Método para obtener el ID como string
    public string? GetIdAsString() => id?.ToString();

    // Método para extraer ID de merchant order
    public string? GetMerchantOrderId()
    {
      if (!string.IsNullOrEmpty(resource) && resource.Contains("merchant_orders/"))
      {
        return resource.Split('/').LastOrDefault();
      }
      return null;
    }
  }

  public class WebhookData
  {
    public object? id { get; set; } // Puede ser string o number

    public string? Id => id?.ToString();
  }

  public class PaymentProcessingResult
  {
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public int SubscriptionId { get; set; }
    public string? UserFcmToken { get; set; }
    public bool SubscriptionActivated { get; set; }
  }
}
