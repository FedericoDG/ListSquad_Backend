using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace listly.Features.Subscription
{
  [Table("payment_mappings")]
  public class PaymentMapping
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("user_uid", TypeName = "varchar(255)")]
    public required string UserUid { get; set; }

    [Column("preference_id", TypeName = "varchar(255)")]
    public required string PreferenceId { get; set; }

    [Column("external_reference", TypeName = "varchar(500)")]
    public required string ExternalReference { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  }
}
