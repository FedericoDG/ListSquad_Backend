using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using listly.Features.Subscription;

namespace listly.Features.User
{
  [Table("users")]
  public class User
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("uid", TypeName = "varchar(255)")]
    public required string UId { get; set; }

    [Column("email", TypeName = "varchar(255)")]
    [EmailAddress]
    public required string Email { get; set; }

    [Column("display_name", TypeName = "varchar(100)")]
    public required string DisplayName { get; set; }

    [Column("photo_url", TypeName = "varchar(500)")]
    public required string PhotoUrl { get; set; }

    [Column("fcm_token", TypeName = "varchar(500)")]
    public string? FcmToken { get; set; }

    // Navegación
    public virtual ICollection<UserSubscription> UserSubscriptions { get; set; } = [];
    public virtual ICollection<Features.List.ListUsers> ListUsers { get; set; } = []; // Listas donde es colaborador
    public virtual Features.Setting.Setting? Setting { get; set; } // Configuraciones
  }
}
