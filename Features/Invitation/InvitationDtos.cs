using System.ComponentModel.DataAnnotations;

namespace listly.Features.Invitation
{
  public class InvitationCreateDto
  {
    [Required(ErrorMessage = "ListId es requerido")]
    public required int ListId { get; set; }
    [Required(ErrorMessage = "ToUserEmail es requerido")]
    [EmailAddress(ErrorMessage = "Dirección de email inválida")]
    public required string ToUserEmail { get; set; }
  }

  public class InvitationUserResponseDto
  {
    [Required(ErrorMessage = "Accepted es requerido")]
    public required bool Accepted { get; set; }
  }

  public class InvitationResponseDto
  {
    public int InvitationId { get; set; }
    public int ListId { get; set; }
    public string FromUserId { get; set; } = default!;
    public string ToUserId { get; set; } = default!;
    public InvitationStatus Status { get; set; }
    public ListResponseDto? List { get; set; }
    public UserResponseDto? FromUser { get; set; }
    public UserResponseDto? ToUser { get; set; }
  }

  public class ListResponseDto
  {
    public int ListId { get; set; }
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string Type { get; set; } = default!;
    public string Icon { get; set; } = default!;
    public string OwnerUid { get; set; } = default!;
  }

  public class UserResponseDto
  {
    public string UId { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string DisplayName { get; set; } = default!;
    public string? PhotoUrl { get; set; }
    public string FcmToken { get; set; } = default!;
  }
}
