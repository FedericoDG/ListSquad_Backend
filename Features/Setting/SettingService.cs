using Microsoft.EntityFrameworkCore;

namespace listly.Features.Setting
{
  public class SettingService
  {
    private readonly ListlyDbContext _context;

    public SettingService(ListlyDbContext context)
    {
      _context = context;
    }

    // Obtener las configuraciones de notificaciones del usuario
    public async Task<SettingDtos.SettingResponseDto?> GetUserSettingsAsync(string userUid)
    {
      var setting = await _context.Settings
        .FirstOrDefaultAsync(s => s.UserUid == userUid);

      if (setting == null)
      {
        // Si no existe configuración para el usuario, crear una por defecto
        setting = new Setting
        {
          UserUid = userUid,
          ReceiveInvitationNotifications = true,
          ReceiveItemAddedNotifications = true,
          ReceiveItemStatusChangedNotifications = true,
          ReceiveItemDeletedNotifications = true
        };

        _context.Settings.Add(setting);
        await _context.SaveChangesAsync();
      }

      return new SettingDtos.SettingResponseDto
      {
        SettingId = setting.SettingId,
        UserUid = setting.UserUid,
        ReceiveInvitationNotifications = setting.ReceiveInvitationNotifications,
        ReceiveItemAddedNotifications = setting.ReceiveItemAddedNotifications,
        ReceiveItemStatusChangedNotifications = setting.ReceiveItemStatusChangedNotifications,
        ReceiveItemDeletedNotifications = setting.ReceiveItemDeletedNotifications
      };
    }

    // Verificar si el usuario debe recibir una notificación específica
    public async Task<bool> ShouldReceiveNotificationAsync(string userUid, string notificationType)
    {
      var setting = await _context.Settings
        .FirstOrDefaultAsync(s => s.UserUid == userUid);

      if (setting == null)
        return true; // Por defecto, todas las notificaciones están habilitadas

      return notificationType switch
      {
        "invitation" => setting.ReceiveInvitationNotifications,
        "item_added" => setting.ReceiveItemAddedNotifications,
        "item_status_changed" => setting.ReceiveItemStatusChangedNotifications,
        "item_deleted" => setting.ReceiveItemDeletedNotifications,
        _ => true
      };
    }

    // Métodos específicos para cada tipo de notificación
    public async Task<bool> GetInvitationNotificationSettingAsync(string userUid)
    {
      var setting = await _context.Settings
        .FirstOrDefaultAsync(s => s.UserUid == userUid);
      return setting?.ReceiveInvitationNotifications ?? true;
    }

    public async Task<bool> GetItemAddedNotificationSettingAsync(string userUid)
    {
      var setting = await _context.Settings
        .FirstOrDefaultAsync(s => s.UserUid == userUid);
      return setting?.ReceiveItemAddedNotifications ?? true;
    }

    public async Task<bool> GetItemStatusChangedNotificationSettingAsync(string userUid)
    {
      var setting = await _context.Settings
        .FirstOrDefaultAsync(s => s.UserUid == userUid);
      return setting?.ReceiveItemStatusChangedNotifications ?? true;
    }

    public async Task<bool> GetItemDeletedNotificationSettingAsync(string userUid)
    {
      var setting = await _context.Settings
        .FirstOrDefaultAsync(s => s.UserUid == userUid);
      return setting?.ReceiveItemDeletedNotifications ?? true;
    }

    // Métodos para actualizar cada configuración por separado
    public async Task<bool> UpdateInvitationNotificationAsync(string userUid, bool enabled)
    {
      var setting = await GetOrCreateSettingAsync(userUid);
      setting.ReceiveInvitationNotifications = enabled;
      await _context.SaveChangesAsync();
      return enabled;
    }

    public async Task<bool> UpdateItemAddedNotificationAsync(string userUid, bool enabled)
    {
      var setting = await GetOrCreateSettingAsync(userUid);
      setting.ReceiveItemAddedNotifications = enabled;
      await _context.SaveChangesAsync();
      return enabled;
    }

    public async Task<bool> UpdateItemStatusChangedNotificationAsync(string userUid, bool enabled)
    {
      var setting = await GetOrCreateSettingAsync(userUid);
      setting.ReceiveItemStatusChangedNotifications = enabled;
      await _context.SaveChangesAsync();
      return enabled;
    }

    public async Task<bool> UpdateItemDeletedNotificationAsync(string userUid, bool enabled)
    {
      var setting = await GetOrCreateSettingAsync(userUid);
      setting.ReceiveItemDeletedNotifications = enabled;
      await _context.SaveChangesAsync();
      return enabled;
    }

    // Método helper para obtener o crear configuración
    private async Task<Setting> GetOrCreateSettingAsync(string userUid)
    {
      var setting = await _context.Settings
        .FirstOrDefaultAsync(s => s.UserUid == userUid);

      if (setting == null)
      {
        setting = new Setting
        {
          UserUid = userUid,
          ReceiveInvitationNotifications = true,
          ReceiveItemAddedNotifications = true,
          ReceiveItemStatusChangedNotifications = true,
          ReceiveItemDeletedNotifications = true
        };

        _context.Settings.Add(setting);
        await _context.SaveChangesAsync();
      }

      return setting;
    }
  }
}
