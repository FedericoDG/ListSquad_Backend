namespace listly.Features.Setting
{
  public class SettingDtos
  {
    public class SettingResponseDto
    {
      public int SettingId { get; set; }
      public required string UserUid { get; set; }
      public bool ReceiveInvitationNotifications { get; set; }
      public bool ReceiveItemAddedNotifications { get; set; }
      public bool ReceiveItemStatusChangedNotifications { get; set; }
      public bool ReceiveItemDeletedNotifications { get; set; }
    }
  }
}
