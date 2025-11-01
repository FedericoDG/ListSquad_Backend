using Microsoft.EntityFrameworkCore;

namespace listly.Features.Item
{
  public class ItemService
  {
    private readonly ListlyDbContext _context;

    public ItemService(ListlyDbContext context)
    {
      _context = context;
    }

    public async Task<Item> CreateItemAsync(ItemDtos.ItemCreateDto dto)
    {
      // Verificar que la lista existe
      var listExists = await _context.Lists.AnyAsync(l => l.ListId == dto.ListId);
      if (!listExists)
      {
        throw new ArgumentException($"La lista con ID {dto.ListId} no existe.");
      }

      var item = new Item
      {
        ListId = dto.ListId,
        Title = dto.Title,
        Completed = dto.Completed,
        Description = dto.Description,
        Notes = dto.Notes,
        CheckedBy = dto.CheckedBy,
        Quantity = dto.Quantity,
        Unit = dto.Unit
      };

      _context.Items.Add(item);
      await _context.SaveChangesAsync();
      return item;
    }

    public async Task<Item?> ToggleCompletedAsync(int itemId, string userUid)
    {
      // Buscar el ítem con su lista
      var item = await _context.Items
        .Include(i => i.List)
        .FirstOrDefaultAsync(i => i.ItemId == itemId);

      if (item == null)
      {
        return null;
      }

      // Verificar que el usuario tenga acceso a la lista del ítem
      var hasAccess = await _context.ListUsers
        .AnyAsync(lu => lu.UId == userUid && lu.ListId == item.ListId);

      if (!hasAccess)
      {
        throw new UnauthorizedAccessException("No tienes acceso a este ítem.");
      }

      // Alternar el estado completed
      item.Completed = !item.Completed;

      // Si se marca como completado, actualizar CheckedBy
      if (item.Completed)
      {
        item.CheckedBy = userUid;
      }
      else
      {
        item.CheckedBy = null; // Opcional: limpiar quien lo marcó como completado
      }

      await _context.SaveChangesAsync();
      return item;
    }

    public async Task<Item?> GetItemByIdAsync(int itemId)
    {
      return await _context.Items
        .Include(i => i.List)
        .FirstOrDefaultAsync(i => i.ItemId == itemId);
    }

    public async Task<Item?> UpdateItemAsync(int itemId, ItemDtos.ItemUpdateDto dto, string userUid)
    {
      // Buscar el ítem con su lista
      var item = await _context.Items
        .Include(i => i.List)
        .FirstOrDefaultAsync(i => i.ItemId == itemId);

      if (item == null)
      {
        return null;
      }

      // Verificar que el usuario tenga acceso a la lista del ítem
      var hasAccess = await _context.ListUsers
        .AnyAsync(lu => lu.UId == userUid && lu.ListId == item.ListId);

      if (!hasAccess)
      {
        throw new UnauthorizedAccessException("No tienes acceso a este ítem.");
      }

      // Actualizar los campos del ítem
      item.Title = dto.Title;
      item.Description = dto.Description;
      item.Notes = dto.Notes;
      item.Quantity = dto.Quantity;
      item.Unit = dto.Unit;

      await _context.SaveChangesAsync();
      return item;
    }

    public async Task<bool> DeleteItemAsync(int itemId, string userUid)
    {
      // Buscar el ítem con su lista
      var item = await _context.Items
        .Include(i => i.List)
        .FirstOrDefaultAsync(i => i.ItemId == itemId);

      if (item == null)
      {
        return false;
      }

      // Verificar que el usuario tenga acceso a la lista del ítem
      var hasAccess = await _context.ListUsers
        .AnyAsync(lu => lu.UId == userUid && lu.ListId == item.ListId);

      if (!hasAccess)
      {
        throw new UnauthorizedAccessException("No tienes acceso a este ítem.");
      }

      // Eliminar el ítem
      _context.Items.Remove(item);
      await _context.SaveChangesAsync();
      return true;
    }
  }
}
