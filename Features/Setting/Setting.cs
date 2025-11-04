using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace listly.Features.Setting
{
  [Table("user_settings")]
  public class Setting
  {
    [Key]
    public int SettingId { get; set; }

    [Required]
    [Column("user_uid", TypeName = "varchar(30)")]
    public required string UserUid { get; set; }

    // Configuraciones de notificaciones (por defecto todas en true)
    public bool ReceiveInvitationNotifications { get; set; } = true;
    public bool ReceiveItemAddedNotifications { get; set; } = true;
    public bool ReceiveItemStatusChangedNotifications { get; set; } = true;
    public bool ReceiveItemDeletedNotifications { get; set; } = true;

    // Navegación
    public User.User User { get; set; } = null!;
  }
}
