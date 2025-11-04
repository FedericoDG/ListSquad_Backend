using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using listly.Features.User;

namespace listly.Features.Item
{
  [Table("items")]
  public class Item
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("item_id")]
    public int ItemId { get; set; }

    [Column("list_id")]
    public required int ListId { get; set; }

    [Column("title", TypeName = "varchar(60)")]
    public required string Title { get; set; }

    [Column("completed")]
    public required bool Completed { get; set; } = false;

    // Opcionales
    [Column("description", TypeName = "varchar(100)")]
    public string? Description { get; set; }

    [Column("notes", TypeName = "varchar(100)")]
    public string? Notes { get; set; }

    [Column("checked_by", TypeName = "varchar(30)")]
    public string? CheckedBy { get; set; }

    [Column("quantity")]
    public int? Quantity { get; set; }

    [Column("unit", TypeName = "varchar(10)")]
    public string? Unit { get; set; }

    // Navegación
    public List.List? List { get; set; }
    public User.User? CheckedUser { get; set; }
  }
}
