namespace listly.Features.List
{
  public class ListDtos
  {
    public class ListCreateDto
    {
      public required string Title { get; set; }
      public string Description { get; set; } = "";
    }

    public class ListResponseDto
    {
      public int ListId { get; set; }
      public required string Title { get; set; }
      public string Description { get; set; } = "";
      public required string Icon { get; set; }
      public required string OwnerUid { get; set; }
    }

    public class ListDetailsResponseDto
    {
      public int ListId { get; set; }
      public required string Title { get; set; }
      public string Description { get; set; } = "";
      public required string Icon { get; set; }
      public required string OwnerUid { get; set; }
      public User.UserDtos.UserResponseDto? Owner { get; set; }
      public List<User.UserDtos.UserResponseDto> Collaborators { get; set; } = [];
      public List<Item.ItemDtos.ItemResponseDto> Items { get; set; } = [];
    }

    public class AddItemToListDto
    {
      public required string Title { get; set; }

      // Opcionales
      public string? Description { get; set; }
      public string? Notes { get; set; }
      public string? CheckedBy { get; set; }
      public int? Quantity { get; set; }
      public string? Unit { get; set; }
    }
  }
}
