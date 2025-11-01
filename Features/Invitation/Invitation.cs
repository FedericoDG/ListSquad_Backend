using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using listly.Features.User;
using listly.Features.List;

namespace listly.Features.Invitation
{
  [JsonConverter(typeof(JsonStringEnumConverter))]
  public enum InvitationStatus
  {
    Pendiente,
    Aceptada,
    Rechazada
  }

  [Table("invitations")]
  public class Invitation
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("invitation_id")]
    public int InvitationId { get; set; }

    [Column("list_id")]
    public int ListId { get; set; }

    [Column("from_uid", TypeName = "varchar(255)")]
    public string FromUserId { get; set; } = default!;

    [Column("to_uid", TypeName = "varchar(255)")]
    public string ToUserId { get; set; } = default!;

    [Column("status", TypeName = "varchar(20)")]
    public InvitationStatus Status { get; set; } = InvitationStatus.Pendiente;

    // Navegación
    public virtual List.List? List { get; set; }
    public virtual User.User? FromUser { get; set; }
    public virtual User.User? ToUser { get; set; }
  }
}
