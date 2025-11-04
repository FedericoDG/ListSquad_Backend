using System.ComponentModel.DataAnnotations;

namespace listly.Features.User
{
  public class UserDtos
  {
    public class UserResponseDto
    {
      public required string UId { get; set; }
      public required string Email { get; set; }
      public required string DisplayName { get; set; }
      public required string PhotoUrl { get; set; }
      public required string FcmToken { get; set; }
    }

    public class UserWithSubscriptionResponseDto
    {
      public required string UId { get; set; }
      public required string Email { get; set; }
      public required string DisplayName { get; set; }
      public required string PhotoUrl { get; set; }
      public required string FcmToken { get; set; }
      public UserSubscriptionDto? Subscription { get; set; }
      public UserSettingsDto? Settings { get; set; }
    }

    public class UserSubscriptionDto
    {
      public int SubscriptionId { get; set; }
      public required string Name { get; set; }
      public required string Description { get; set; }
      public decimal Price { get; set; }
      public DateTime StartDate { get; set; }
      public DateTime EndDate { get; set; }
      public bool IsActive { get; set; }
    }

    public class UserSettingsDto
    {
      public int SettingId { get; set; }
      public bool ReceiveInvitationNotifications { get; set; }
      public bool ReceiveItemAddedNotifications { get; set; }
      public bool ReceiveItemStatusChangedNotifications { get; set; }
      public bool ReceiveItemDeletedNotifications { get; set; }
    }
  }
}
