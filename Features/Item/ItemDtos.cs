namespace listly.Features.Item
{
  public class ItemDtos
  {
    public class ItemCreateDto
    {
      public required int ListId { get; set; }
      public required string Title { get; set; }
      public required bool Completed { get; set; }

      // Opcionales
      public string? Description { get; set; }
      public string? Notes { get; set; }
      public string? CheckedBy { get; set; }
      public int? Quantity { get; set; }
      public string? Unit { get; set; }
    }

    public class ItemUpdateDto
    {
      public required string Title { get; set; }

      // Opcionales
      public string? Description { get; set; }
      public string? Notes { get; set; }
      public int? Quantity { get; set; }
      public string? Unit { get; set; }
    }

    public class ItemResponseDto
    {
      public int ItemId { get; set; }
      public required int ListId { get; set; }
      public required string Title { get; set; }
      public required bool Completed { get; set; }

      // Opcionales
      public string? Description { get; set; }
      public string? Notes { get; set; }
      public string? CheckedBy { get; set; }
      public int? Quantity { get; set; }
      public string? Unit { get; set; }
    }
  }
}
