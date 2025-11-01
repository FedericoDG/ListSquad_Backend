using System.ComponentModel.DataAnnotations;

namespace listly.Features.Auth
{
  public class AuthDtos
  {
    public class LoginRequestDto
    {
      [Required(ErrorMessage = "El UID es requerido")]
      public required string UId { get; set; }

      [Required(ErrorMessage = "El email es requerido")]
      [EmailAddress(ErrorMessage = "El formato del email no es válido")]
      public required string Email { get; set; }

      [Required(ErrorMessage = "El nombre de usuario es requerido")]
      public required string DisplayName { get; set; }

      [Required(ErrorMessage = "La URL de la foto es requerida")]
      [Url(ErrorMessage = "El formato de la URL no es válido")]
      public required string PhotoUrl { get; set; }

      [Required(ErrorMessage = "El ID del proveedor es requerido")]
      public required string ProviderId { get; set; }

      [Required(ErrorMessage = "El token FCM es requerido")]
      public required string FcmToken { get; set; }

    }

    public class RebootRequestDto
    {
      [Required(ErrorMessage = "El UID es requerido")]
      public required string UId { get; set; }

      public string FcmToken { get; set; }
    }

    public class AuthResponseDto
    {
      public required string UId { get; set; }
      public required string Token { get; set; }
    }

    public class RebootResponseDto
    {
      public required bool Ok { get; set; }
    }

    public class TestNotificationDto
    {
      public string? Title { get; set; }
      public string? Body { get; set; }
      public string? ClickAction { get; set; }
      public Dictionary<string, string>? Data { get; set; }
    }
  }
}
