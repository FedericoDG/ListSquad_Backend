
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace listly.Features.Subscription
{
  [Table("subscriptions")]
  public class Subscription
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int SubscriptionId { get; set; }

    [Column("name", TypeName = "varchar(50)")]
    public required string Name { get; set; }

    [Column("description", TypeName = "varchar(200)")]
    public required string Description { get; set; }

    [Column("price", TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    // Navegación
    public ICollection<UserSubscription> UserSubscriptions { get; set; } = [];
  }

  [Table("user_subscriptions")]
  public class UserSubscription
  {
    [Column("uid", TypeName = "varchar(30)")]
    public string UId { get; set; } = default!;

    [Column("subscription_id")]
    public int SubscriptionId { get; set; }

    [Column("start_date")]
    public DateTime StartDate { get; set; }

    [Column("end_date")]
    public DateTime EndDate { get; set; }

    // Navegación
    public User.User? User { get; set; }
    public Subscription? Subscription { get; set; }
  }
}
