using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace listly.Features.Setting
{
  [Route("api/settings")]
  [ApiController]
  [Authorize]
  public class SettingController : ControllerBase
  {
    private readonly SettingService _settingService;

    public SettingController(SettingService settingService)
    {
      _settingService = settingService;
    }

    // Obtener las configuraciones de notificaciones del usuario
    [HttpGet("invitation-notifications")]
    public async Task<IActionResult> GetInvitationNotificationSetting()
    {
      var userUid = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (string.IsNullOrEmpty(userUid))
        return Unauthorized();

      var enabled = await _settingService.GetInvitationNotificationSettingAsync(userUid);
      return Ok(new { enabled });
    }

    // Obtener las configuraciones de notificaciones del usuario
    [HttpGet("item-added-notifications")]
    public async Task<IActionResult> GetItemAddedNotificationSetting()
    {
      var userUid = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (string.IsNullOrEmpty(userUid))
        return Unauthorized();

      var enabled = await _settingService.GetItemAddedNotificationSettingAsync(userUid);
      return Ok(new { enabled });
    }

    //  Obtener las configuraciones de notificaciones del usuario
    [HttpGet("item-status-changed-notifications")]
    public async Task<IActionResult> GetItemStatusChangedNotificationSetting()
    {
      var userUid = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (string.IsNullOrEmpty(userUid))
        return Unauthorized();

      var enabled = await _settingService.GetItemStatusChangedNotificationSettingAsync(userUid);
      return Ok(new { enabled });
    }

    //  Obtener las configuraciones de notificaciones del usuario
    [HttpGet("item-deleted-notifications")]
    public async Task<IActionResult> GetItemDeletedNotificationSetting()
    {
      var userUid = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (string.IsNullOrEmpty(userUid))
        return Unauthorized();

      var enabled = await _settingService.GetItemDeletedNotificationSettingAsync(userUid);
      return Ok(new { enabled });
    }

    // Actualizar las configuraciones de notificaciones del usuario
    [HttpPut("invitation-notifications")]
    public async Task<IActionResult> UpdateInvitationNotificationSetting([FromBody] bool enabled)
    {
      var userUid = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (string.IsNullOrEmpty(userUid))
        return Unauthorized();

      try
      {
        var result = await _settingService.UpdateInvitationNotificationAsync(userUid, enabled);
        return Ok(new { enabled = result });
      }
      catch (Exception ex)
      {
        return BadRequest(new { message = ex.Message });
      }
    }

    // Actualizar las configuraciones de notificaciones del usuario
    [HttpPut("item-added-notifications")]
    public async Task<IActionResult> UpdateItemAddedNotificationSetting([FromBody] bool enabled)
    {
      var userUid = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (string.IsNullOrEmpty(userUid))
        return Unauthorized();

      try
      {
        var result = await _settingService.UpdateItemAddedNotificationAsync(userUid, enabled);
        return Ok(new { enabled = result });
      }
      catch (Exception ex)
      {
        return BadRequest(new { message = ex.Message });
      }
    }

    // Actualizar las configuraciones de notificaciones del usuario
    [HttpPut("item-status-changed-notifications")]
    public async Task<IActionResult> UpdateItemStatusChangedNotificationSetting([FromBody] bool enabled)
    {
      var userUid = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (string.IsNullOrEmpty(userUid))
        return Unauthorized();

      try
      {
        var result = await _settingService.UpdateItemStatusChangedNotificationAsync(userUid, enabled);
        return Ok(new { enabled = result });
      }
      catch (Exception ex)
      {
        return BadRequest(new { message = ex.Message });
      }
    }

    // Actualizar las configuraciones de notificaciones del usuario
    [HttpPut("item-deleted-notifications")]
    public async Task<IActionResult> UpdateItemDeletedNotificationSetting([FromBody] bool enabled)
    {
      var userUid = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (string.IsNullOrEmpty(userUid))
        return Unauthorized();

      try
      {
        var result = await _settingService.UpdateItemDeletedNotificationAsync(userUid, enabled);
        return Ok(new { enabled = result });
      }
      catch (Exception ex)
      {
        return BadRequest(new { message = ex.Message });
      }
    }
  }
}
