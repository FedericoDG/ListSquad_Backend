using Microsoft.EntityFrameworkCore;
using listly.Features.User;

namespace listly.Features.Invitation
{
  public class InvitationService
  {
    private readonly ListlyDbContext _context;
    private readonly UserServices _userServices;

    public InvitationService(ListlyDbContext context, UserServices userServices)
    {
      _context = context;
      _userServices = userServices;
    }

    // Crear una nueva invitación
    public async Task<InvitationResponseDto> CreateInvitationAsync(InvitationCreateDto dto, string fromUserId)
    {
      // Buscar el UId del usuario destinatario por email
      var toUserId = await _userServices.GetUserUIdByEmailAsync(dto.ToUserEmail);

      if (toUserId == null)
      {
        throw new ArgumentException("Usuario no encontrado");
      }

      // Verificar si ya existe una invitación pendiente para esta lista y usuario
      var existingInvitation = await _context.Invitations
        .FirstOrDefaultAsync(i => i.ListId == dto.ListId && i.ToUserId == toUserId && i.Status == InvitationStatus.Pendiente);

      if (existingInvitation != null)
      {
        throw new InvalidOperationException("Ya existe una invitación pendiente para este usuario en esta lista");
      }

      var invitation = new Invitation
      {
        ListId = dto.ListId,
        FromUserId = fromUserId,
        ToUserId = toUserId,
        Status = InvitationStatus.Pendiente
      };

      _context.Invitations.Add(invitation);
      await _context.SaveChangesAsync();

      // Cargar la invitación con los usuarios relacionados
      var invitationWithUsers = await _context.Invitations
        .Include(i => i.List)
        .Include(i => i.FromUser)
        .Include(i => i.ToUser)
        .FirstAsync(i => i.InvitationId == invitation.InvitationId);

      // Mapear a DTO de respuesta
      var responseDto = new InvitationResponseDto
      {
        InvitationId = invitationWithUsers.InvitationId,
        ListId = invitationWithUsers.ListId,
        FromUserId = invitationWithUsers.FromUserId,
        ToUserId = invitationWithUsers.ToUserId,
        Status = invitationWithUsers.Status,
        List = invitationWithUsers.List != null ? new ListResponseDto
        {
          ListId = invitationWithUsers.List.ListId,
          Title = invitationWithUsers.List.Title,
          Description = invitationWithUsers.List.Description,
          Icon = invitationWithUsers.List.Icon,
          OwnerUid = invitationWithUsers.List.OwnerUid
        } : null,
        FromUser = invitationWithUsers.FromUser != null ? new UserResponseDto
        {
          UId = invitationWithUsers.FromUser.UId,
          Email = invitationWithUsers.FromUser.Email,
          DisplayName = invitationWithUsers.FromUser.DisplayName,
          PhotoUrl = invitationWithUsers.FromUser.PhotoUrl,
          ProviderId = invitationWithUsers.FromUser.ProviderId,
          FcmToken = invitationWithUsers.FromUser.FcmToken ?? ""
        } : null,
        ToUser = invitationWithUsers.ToUser != null ? new UserResponseDto
        {
          UId = invitationWithUsers.ToUser.UId,
          Email = invitationWithUsers.ToUser.Email,
          DisplayName = invitationWithUsers.ToUser.DisplayName,
          PhotoUrl = invitationWithUsers.ToUser.PhotoUrl,
          ProviderId = invitationWithUsers.ToUser.ProviderId,
          FcmToken = invitationWithUsers.ToUser.FcmToken ?? ""
        } : null
      };

      return responseDto;
    }

    // Obtener invitaciones pendientes para el usuario autenticado
    public async Task<List<InvitationResponseDto>> GetPendingInvitationsAsync(string toUserId)
    {
      var invitations = await _context.Invitations
        .Include(i => i.List)
        .Include(i => i.FromUser)
        .Include(i => i.ToUser)
        .Where(i => i.ToUserId == toUserId && i.Status == InvitationStatus.Pendiente)
        .ToListAsync();

      var responseDtos = invitations.Select(invitation => new InvitationResponseDto
      {
        InvitationId = invitation.InvitationId,
        ListId = invitation.ListId,
        FromUserId = invitation.FromUserId,
        ToUserId = invitation.ToUserId,
        Status = invitation.Status,
        List = invitation.List != null ? new ListResponseDto
        {
          ListId = invitation.List.ListId,
          Title = invitation.List.Title,
          Description = invitation.List.Description,
          Icon = invitation.List.Icon,
          OwnerUid = invitation.List.OwnerUid
        } : null,
        FromUser = invitation.FromUser != null ? new UserResponseDto
        {
          UId = invitation.FromUser.UId,
          Email = invitation.FromUser.Email,
          DisplayName = invitation.FromUser.DisplayName,
          PhotoUrl = invitation.FromUser.PhotoUrl,
          ProviderId = invitation.FromUser.ProviderId,
          FcmToken = invitation.FromUser.FcmToken ?? ""
        } : null,
        ToUser = invitation.ToUser != null ? new UserResponseDto
        {
          UId = invitation.ToUser.UId,
          Email = invitation.ToUser.Email,
          DisplayName = invitation.ToUser.DisplayName,
          PhotoUrl = invitation.ToUser.PhotoUrl,
          ProviderId = invitation.ToUser.ProviderId,
          FcmToken = invitation.ToUser.FcmToken ?? ""
        } : null
      }).ToList();

      return responseDtos;
    }

    // Responder a una invitación (aceptar o rechazar)
    public async Task<bool> RespondToInvitationAsync(int invitationId, string userId, bool accepted)
    {
      // Buscar la invitación
      var invitation = await _context.Invitations
        .Include(i => i.List)
        .FirstOrDefaultAsync(i => i.InvitationId == invitationId && i.ToUserId == userId && i.Status == InvitationStatus.Pendiente);

      if (invitation == null)
      {
        return false; // Invitación no encontrada o no pertenece al usuario
      }

      if (accepted)
      {
        // Verificar si el usuario ya es colaborador de la lista
        var existingCollaborator = await _context.ListUsers
          .FirstOrDefaultAsync(lu => lu.ListId == invitation.ListId && lu.UId == userId);

        if (existingCollaborator == null)
        {
          // Agregar el usuario como colaborador de la lista
          var newCollaborator = new Features.List.ListUsers
          {
            ListId = invitation.ListId,
            UId = userId
          };

          _context.ListUsers.Add(newCollaborator);
        }
      }

      // Eliminar la invitación independientemente de si fue aceptada o rechazada
      _context.Invitations.Remove(invitation);

      await _context.SaveChangesAsync();
      return true;
    }
  }
}
