using Microsoft.EntityFrameworkCore;
using listly.Features.User;

namespace listly.Features.List
{
  public class ListService
  {
    private readonly ListlyDbContext _context;

    public ListService(ListlyDbContext context)
    {
      _context = context;
    }

    public async Task<ListDtos.ListResponseDto> CreateListAsync(ListDtos.ListCreateDto dto, string ownerUid)
    {
      // Asignar icono por defecto
      var icon = "🛒";

      var list = new List
      {
        Title = dto.Title,
        Description = dto.Description,
        Icon = icon,
        OwnerUid = ownerUid
      };
      _context.Lists.Add(list);
      await _context.SaveChangesAsync();

      // Agregar al owner como colaborador de la lista
      var listUser = new ListUsers
      {
        UId = ownerUid,
        ListId = list.ListId
      };
      _context.ListUsers.Add(listUser);
      await _context.SaveChangesAsync();

      // Crear el DTO de respuesta
      var responseDto = new ListDtos.ListResponseDto
      {
        ListId = list.ListId,
        Title = list.Title,
        Description = list.Description,
        Icon = list.Icon,
        OwnerUid = list.OwnerUid
      };

      return responseDto;
    }

    public async Task<List<ListDtos.ListResponseDto>> GetUserListsAsync(string userUid)
    {
      // Obtener todas las listas donde el usuario es colaborador con proyección directa
      var responseDtos = await _context.ListUsers
        .Where(lu => lu.UId == userUid)
        .OrderByDescending(lu => lu.List!.ListId)
        .Select(lu => new ListDtos.ListResponseDto
        {
          ListId = lu.List!.ListId,
          Title = lu.List.Title,
          Description = lu.List.Description,
          Icon = lu.List.Icon,
          OwnerUid = lu.List.OwnerUid,
        })
        .ToListAsync();

      return responseDtos;
    }

    public async Task<ListDtos.ListDetailsResponseDto?> GetListByIdAsync(int listId, string userUid)
    {
      // Verificar que el usuario tenga acceso a la lista
      var hasAccess = await _context.ListUsers
        .AnyAsync(lu => lu.UId == userUid && lu.ListId == listId);

      if (!hasAccess)
      {
        return null; // Usuario no tiene acceso a esta lista
      }

      // Obtener la lista con todos los detalles
      var list = await _context.Lists
        .Include(l => l.ListUsers)
        .ThenInclude(lu => lu.User)
        .Include(l => l.Items)
        .FirstOrDefaultAsync(l => l.ListId == listId);

      if (list == null)
      {
        return null;
      }

      // Obtener datos del owner
      var owner = await _context.User
        .FirstOrDefaultAsync(u => u.UId == list.OwnerUid);

      User.UserDtos.UserResponseDto? ownerDto = null;
      if (owner != null)
      {
        ownerDto = new User.UserDtos.UserResponseDto
        {
          UId = owner.UId,
          Email = owner.Email,
          DisplayName = owner.DisplayName,
          PhotoUrl = owner.PhotoUrl,
          FcmToken = owner.FcmToken ?? ""
        };
      }

      // Obtener colaboradores (excluyendo al owner)
      var collaborators = list.ListUsers
        .Where(lu => lu.UId != list.OwnerUid) // Filtrar para excluir al owner
        .Select(lu => new User.UserDtos.UserResponseDto
        {
          UId = lu.User!.UId,
          Email = lu.User.Email,
          DisplayName = lu.User.DisplayName,
          PhotoUrl = lu.User.PhotoUrl,
          FcmToken = lu.User.FcmToken ?? ""
        })
        .ToList();

      // Obtener items
      var items = list.Items
        .Select(i => new Item.ItemDtos.ItemResponseDto
        {
          ItemId = i.ItemId,
          ListId = i.ListId,
          Title = i.Title,
          Completed = i.Completed,
          Description = i.Description,
          Notes = i.Notes,
          CheckedBy = i.CheckedBy,
          Quantity = i.Quantity,
          Unit = i.Unit
        })
        .ToList();

      return new ListDtos.ListDetailsResponseDto
      {
        ListId = list.ListId,
        Title = list.Title,
        Description = list.Description,
        Icon = list.Icon,
        OwnerUid = list.OwnerUid,
        Owner = ownerDto,
        Collaborators = collaborators,
        Items = items
      };
    }

    public async Task DeleteListAsync(int listId, string userUid)
    {
      // Buscar la lista
      var list = await _context.Lists.FindAsync(listId);

      if (list == null)
      {
        throw new KeyNotFoundException($"La lista con ID {listId} no existe.");
      }

      // Verificar que el usuario sea el owner
      if (list.OwnerUid != userUid)
      {
        throw new UnauthorizedAccessException("Solo el propietario de la lista puede eliminarla.");
      }

      // Eliminar la lista
      _context.Lists.Remove(list);
      await _context.SaveChangesAsync();
    }

    public async Task<bool> IsUserOwnerOfListAsync(int listId, string userUid)
    {
      var list = await _context.Lists.FindAsync(listId);
      return list != null && list.OwnerUid == userUid;
    }

    public async Task RemoveCollaboratorAsync(int listId, string collaboratorUid, string requesterUid)
    {
      // Buscar la lista
      var list = await _context.Lists.FindAsync(listId);

      if (list == null)
      {
        throw new KeyNotFoundException($"La lista con ID {listId} no existe.");
      }

      // Verificar que el usuario que hace la petición sea el owner
      if (list.OwnerUid != requesterUid)
      {
        throw new UnauthorizedAccessException("Solo el propietario de la lista puede eliminar colaboradores.");
      }

      // Verificar que no se esté intentando eliminar al owner
      if (list.OwnerUid == collaboratorUid)
      {
        throw new InvalidOperationException("No se puede eliminar al propietario de la lista.");
      }

      // Buscar la relación de colaborador
      var listUser = await _context.ListUsers
        .FirstOrDefaultAsync(lu => lu.ListId == listId && lu.UId == collaboratorUid);

      if (listUser == null)
      {
        throw new KeyNotFoundException("El usuario no es colaborador de esta lista.");
      }

      // Eliminar la relación
      _context.ListUsers.Remove(listUser);
      await _context.SaveChangesAsync();
    }
  }
}
