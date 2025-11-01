using Microsoft.EntityFrameworkCore;
using listly.Features.Subscription;
using listly.Features.Setting;
using UserModel = listly.Features.User.User;

namespace listly.Features.Auth
{
  public class AuthServices
  {
    private readonly ListlyDbContext _context;
    private readonly SettingService _settingService;

    public AuthServices(ListlyDbContext context, SettingService settingService)
    {
      _context = context;
      _settingService = settingService;
    }

    public async Task<UserModel?> GetUserByUIdAsync(string uid)
    {
      return await _context.User.FirstOrDefaultAsync(u => u.UId == uid);
    }

    public async Task<UserModel> RegisterUserAsync(AuthDtos.LoginRequestDto dto)
    {
      var user = new UserModel
      {
        UId = dto.UId,
        Email = dto.Email,
        DisplayName = dto.DisplayName,
        PhotoUrl = dto.PhotoUrl,
        ProviderId = dto.ProviderId,
        FcmToken = dto.FcmToken,
      };
      _context.User.Add(user);
      await _context.SaveChangesAsync();

      // Crear suscripción gratuita por defecto
      var userSubscription = new UserSubscription
      {
        UId = user.UId,
        SubscriptionId = 1 // Suscripción gratuita por defecto
      };
      _context.UserSubscriptions.Add(userSubscription);

      // Crear configuraciones por defecto
      var userSettings = new listly.Features.Setting.Setting
      {
        UserUid = user.UId,
        ReceiveInvitationNotifications = true,
        ReceiveItemAddedNotifications = true,
        ReceiveItemStatusChangedNotifications = true,
        ReceiveItemDeletedNotifications = true
      };
      _context.Settings.Add(userSettings);

      await _context.SaveChangesAsync();

      return user;
    }

    public async Task<UserModel> UpdateFcmTokenAsync(string uid, string fcmToken)
    {
      var user = await GetUserByUIdAsync(uid) ?? throw new Exception("User not found");
      user.FcmToken = fcmToken;
      _context.User.Update(user);
      await _context.SaveChangesAsync();
      return user;
    }
  }
}
