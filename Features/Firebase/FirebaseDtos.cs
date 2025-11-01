using System.ComponentModel.DataAnnotations;

namespace listly.Features.Firebase
{
  public class NotificationRequest
  {
    [Required(ErrorMessage = "El token FCM es requerido")]
    public required string FcmToken { get; set; }

    public string? Title { get; set; }

    public string? Body { get; set; }

    public string? ClickAction { get; set; }

    public Dictionary<string, string>? Data { get; set; }
  }

  public class NotificationResponse
  {
    public bool Success { get; set; }
    public string? MessageId { get; set; }
    public string? Error { get; set; }
  }

  public class MultipleNotificationResponse
  {
    public bool Success { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<string>? Errors { get; set; }
  }
}
