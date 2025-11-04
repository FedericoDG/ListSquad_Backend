using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using static listly.Features.User.UserDtos;

namespace listly.Features.User
{
  [Route("api/users")]
  [Authorize]
  [ApiController]
  public class UserController : ControllerBase
  {
    private readonly UserServices _userServices;
    private readonly listly.Features.Setting.SettingService _settingService;

    public UserController(UserServices userServices, listly.Features.Setting.SettingService settingService)
    {
      _userServices = userServices;
      _settingService = settingService;
    }

    // Obtener información del usuario autenticado junto con su suscripción activa y configuraciones
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
      var uid = User.FindFirst("uid")?.Value;

      if (string.IsNullOrEmpty(uid))
      {
        return Unauthorized("Token inválido");
      }

      var user = await _userServices.GetUserWithActiveSubscriptionAsync(uid);

      if (user == null)
      {
        return NotFound("Usuario no encontrado");
      }

      // Obtener la suscripción activa (fecha actual entre start_date y end_date)
      var now = DateTime.UtcNow;
      var activeSubscription = user.UserSubscriptions
        .FirstOrDefault(us => us.StartDate <= now && us.EndDate >= now);

      // Si el usuario no tiene settings, crear las por defecto usando el servicio
      if (user.Setting == null)
      {
        var settings = await _settingService.GetUserSettingsAsync(user.UId);
        // El servicio GetUserSettingsAsync ya crea settings por defecto si no existen
        if (settings != null)
        {
          // Recargar el usuario para incluir los settings recién creados
          var updatedUser = await _userServices.GetUserWithActiveSubscriptionAsync(uid);
          if (updatedUser != null)
          {
            user = updatedUser;
          }
        }
      }

      var response = new UserWithSubscriptionResponseDto
      {
        UId = user.UId,
        Email = user.Email,
        DisplayName = user.DisplayName,
        PhotoUrl = user.PhotoUrl,
        FcmToken = user.FcmToken ?? "",
        Subscription = activeSubscription != null ? new UserSubscriptionDto
        {
          SubscriptionId = activeSubscription.SubscriptionId,
          Name = activeSubscription.Subscription?.Name ?? "",
          Description = activeSubscription.Subscription?.Description ?? "",
          Price = activeSubscription.Subscription?.Price ?? 0m,
          StartDate = activeSubscription.StartDate,
          EndDate = activeSubscription.EndDate,
          IsActive = true
        } : null,
        Settings = user.Setting != null ? new UserSettingsDto
        {
          SettingId = user.Setting.SettingId,
          ReceiveInvitationNotifications = user.Setting.ReceiveInvitationNotifications,
          ReceiveItemAddedNotifications = user.Setting.ReceiveItemAddedNotifications,
          ReceiveItemStatusChangedNotifications = user.Setting.ReceiveItemStatusChangedNotifications,
          ReceiveItemDeletedNotifications = user.Setting.ReceiveItemDeletedNotifications
        } : null
      };

      return Ok(response);
    }
  }
}
