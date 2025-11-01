using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using listly.Features.Firebase;
using static listly.Features.Auth.AuthDtos;

namespace listly.Features.Auth
{
  [Route("api/auth")]
  [ApiController]
  public class AuthController : ControllerBase
  {

    private AuthServices _authServices;
    private JwtService _jwtService;
    private readonly FirebaseService? _firebaseService;

    public AuthController(AuthServices authServices, JwtService jwtService, FirebaseService? firebaseService = null)
    {
      _authServices = authServices;
      _jwtService = jwtService;
      _firebaseService = firebaseService;
    }


    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] LoginRequestDto dto)
    {
      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      var user = await _authServices.GetUserByUIdAsync(dto.UId);

      user ??= await _authServices.RegisterUserAsync(dto);

      if (user.FcmToken != dto.FcmToken)
      {
        await _authServices.UpdateFcmTokenAsync(user.UId, dto.FcmToken);
      }

      var jwt = _jwtService.GenerateJwtToken(user.UId);

      return Ok(new AuthResponseDto { UId = user.UId, Token = jwt });
    }

    [Authorize]
    [HttpPost("test-notification")]
    public async Task<IActionResult> TestNotification([FromBody] TestNotificationDto dto)
    {
      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (string.IsNullOrEmpty(userId))
      {
        return Unauthorized("Usuario no autenticado correctamente");
      }

      var user = await _authServices.GetUserByUIdAsync(userId);
      if (user == null)
      {
        return NotFound("Usuario no encontrado");
      }

      if (string.IsNullOrEmpty(user.FcmToken))
      {
        return BadRequest("Usuario no tiene FCM Token configurado");
      }

      if (_firebaseService == null)
      {
        return StatusCode(500, "Servicio de Firebase no disponible");
      }

      try
      {
        await _firebaseService.SendNotificationAsync(new Firebase.NotificationRequest
        {
          FcmToken = user.FcmToken,
          Title = dto.Title ?? "🧪 Notificación de prueba",
          Body = dto.Body ?? "Esta es una notificación de prueba desde la API.",
          ClickAction = dto.ClickAction ?? "OPEN_MAIN_ACTIVITY",
          Data = dto.Data ?? new Dictionary<string, string>
          {
            ["type"] = "test_notification",
            ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
          }
        });

        return Ok(new
        {
          success = true,
          message = "Notificación enviada exitosamente",
          userEmail = user.Email,
          hasFcmToken = !string.IsNullOrEmpty(user.FcmToken)
        });
      }
      catch (Exception ex)
      {
        return StatusCode(500, new
        {
          success = false,
          message = "Error enviando notificación",
          error = ex.Message
        });
      }
    }
  }
}
