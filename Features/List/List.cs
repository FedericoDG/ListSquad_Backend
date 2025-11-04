using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using listly.Features.User;

namespace listly.Features.List
{
  [Table("lists")]
  public class List
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("list_id")]
    public int ListId { get; set; }

    [Column("title", TypeName = "varchar(60)")]
    public required string Title { get; set; }

    [Column("description", TypeName = "varchar(100)")]
    public string Description { get; set; } = "";

    [Column("icon", TypeName = "varchar(10)")]
    public required string Icon { get; set; }

    [Column("owner_uid", TypeName = "varchar(30)")]
    public required string OwnerUid { get; set; }

    // Navegación
    public virtual User.User? Owner { get; set; }
    public virtual ICollection<ListUsers> ListUsers { get; set; } = [];
    public virtual ICollection<Item.Item> Items { get; set; } = [];
  }

  [Table("list_users")]
  public class ListUsers
  {
    [Column("uid", TypeName = "varchar(255)")]
    public string UId { get; set; } = default!;

    [Column("list_id")]
    public int ListId { get; set; }

    // Navegación
    public virtual User.User? User { get; set; }
    public virtual List? List { get; set; }
  }
}
