using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using listly.Features.Firebase;
using listly.Features.Setting;

namespace listly.Features.Invitation
{
  [ApiController]
  [Route("api/invitations")]
  [Authorize]
  public class InvitationController : ControllerBase
  {
    private readonly InvitationService _invitationService;
    private readonly FirebaseService _firebaseService;
    private readonly SettingService _settingService;

    public InvitationController(InvitationService invitationService, FirebaseService firebaseService, SettingService settingService)
    {
      _invitationService = invitationService;
      _firebaseService = firebaseService;
      _settingService = settingService;
    }

    // Crear una nueva invitación
    [HttpPost]
    public async Task<IActionResult> CreateInvitation([FromBody] InvitationCreateDto dto)
    {
      var fromUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (string.IsNullOrEmpty(fromUserId))
        return Unauthorized();

      try
      {
        var invitation = await _invitationService.CreateInvitationAsync(dto, fromUserId);

        // Enviar notificación push si el usuario tiene FCM token y tiene habilitadas las notificaciones de invitación
        if (!string.IsNullOrEmpty(invitation.ToUser?.FcmToken))
        {
          // Verificar si el usuario quiere recibir notificaciones de invitación
          var receiveInvitationNotifications = await _settingService.GetInvitationNotificationSettingAsync(invitation.ToUser.UId);

          if (receiveInvitationNotifications)
          {
            try
            {
              await _firebaseService.SendNotificationAsync(new NotificationRequest
              {
                FcmToken = invitation.ToUser.FcmToken,
                Title = "📋 Nueva invitación",
                Body = $"{invitation.FromUser?.DisplayName ?? "Alguien"} te ha invitado a '{invitation.List?.Title ?? "una lista"}'",
                ClickAction = "OPEN_INVITATIONS",
                Data = new Dictionary<string, string>
                {
                  ["type"] = "invitations",
                }
              });
            }
            catch (Exception notificationEx)
            {
              // Log el error pero no fallar la creación de la invitación
              Console.WriteLine($"Error enviando notificación: {notificationEx.Message}");
            }
          }
        }

        return Ok(invitation);
      }
      catch (ArgumentException ex) when (ex.Message.Contains("no encontrado"))
      {
        return NotFound(ex.Message);
      }
      catch (InvalidOperationException ex) when (ex.Message.Contains("Ya existe"))
      {
        return BadRequest(ex.Message);
      }
      catch (ArgumentException ex)
      {
        return BadRequest(ex.Message);
      }
    }

    // Obtener invitaciones pendientes para el usuario autenticado
    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingInvitations()
    {
      var toUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (string.IsNullOrEmpty(toUserId))
        return Unauthorized();

      try
      {
        var invitations = await _invitationService.GetPendingInvitationsAsync(toUserId);
        return Ok(invitations);
      }
      catch (Exception ex)
      {
        return StatusCode(500, $"Error interno del servidor: {ex.Message}");
      }
    }


    // Responder a una invitación (aceptar o rechazar)
    [HttpPost("{invitationId}/respond")]
    public async Task<IActionResult> RespondToInvitation(int invitationId, [FromBody] InvitationUserResponseDto dto)
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (string.IsNullOrEmpty(userId))
        return Unauthorized();

      Console.WriteLine($"RespondToInvitation called with invitationId: {invitationId}, accepted: {dto.Accepted}, userId: {userId}");
      try
      {
        var result = await _invitationService.RespondToInvitationAsync(invitationId, userId, dto.Accepted);

        if (result)
        {
          return Ok(new { accepted = dto.Accepted });
        }
        else
        {
          return NotFound("Invitación no encontrada o no tienes acceso a ella.");
        }
      }
      catch (ArgumentException ex)
      {
        return BadRequest(ex.Message);
      }
      catch (Exception ex)
      {
        return StatusCode(500, $"Error interno del servidor: {ex.Message}");
      }
    }
  }
}
