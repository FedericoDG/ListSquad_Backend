using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using listly.Features.Item;
using listly.Features.Firebase;
using listly.Features.Setting;

namespace listly.Features.List
{
  [Route("api/lists")]
  [ApiController]
  [Authorize]
  public class ListController : ControllerBase
  {
    private readonly ListService _listService;
    private readonly ItemService _itemService;
    private readonly FirebaseService _firebaseService;
    private readonly SettingService _settingService;

    public ListController(ListService listService, ItemService itemService, FirebaseService firebaseService, SettingService settingService)
    {
      _listService = listService;
      _itemService = itemService;
      _firebaseService = firebaseService;
      _settingService = settingService;
    }

    // Crear una nueva lista
    [HttpPost]
    public async Task<IActionResult> CreateList([FromBody] ListDtos.ListCreateDto dto)
    {
      var ownerUid = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (string.IsNullOrEmpty(ownerUid))
        return Unauthorized();

      var listResponse = await _listService.CreateListAsync(dto, ownerUid);
      return Ok(listResponse);
    }

    // Obtener todas las listas en las que participo
    [HttpGet]
    public async Task<IActionResult> GetMyLists()
    {
      var ownerUid = User.FindFirstValue(ClaimTypes.NameIdentifier);

      if (string.IsNullOrEmpty(ownerUid))
        return Unauthorized();

      var lists = await _listService.GetUserListsAsync(ownerUid);
      return Ok(lists);
    }

    // Obtener detalles de una lista específica
    [HttpGet("{listId}")]
    public async Task<IActionResult> GetListById(int listId)
    {
      var userUid = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (string.IsNullOrEmpty(userUid))
        return Unauthorized();

      var listDetails = await _listService.GetListByIdAsync(listId, userUid);
      if (listDetails == null)
        return NotFound(new { message = "Lista no encontrada o no tienes acceso a ella." });

      return Ok(listDetails);
    }

    // Agregar un item a una lista
    [HttpPost("{listId}/items")]
    public async Task<IActionResult> AddItemToList(int listId, [FromBody] ListDtos.AddItemToListDto dto)
    {
      var userUid = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (string.IsNullOrEmpty(userUid))
        return Unauthorized();

      try
      {
        // Mapear AddItemToListDto a ItemCreateDto
        var itemDto = new ItemDtos.ItemCreateDto
        {
          ListId = listId,
          Title = dto.Title,
          Completed = false, // Los ítems nuevos siempre empiezan como no completados
          Description = dto.Description,
          Notes = dto.Notes,
          CheckedBy = dto.CheckedBy,
          Quantity = dto.Quantity,
          Unit = dto.Unit
        };

        var item = await _itemService.CreateItemAsync(itemDto);

        // Obtener detalles de la lista para las notificaciones
        var listDetails = await _listService.GetListByIdAsync(listId, userUid);
        if (listDetails != null)
        {
          // Obtener el nombre del usuario que agregó el item
          var addedByUser = listDetails.Collaborators.FirstOrDefault(c => c.UId == userUid)
                           ?? listDetails.Owner;

          // Crear lista de usuarios a notificar (todos los colaboradores excepto quien agregó el item)
          var usersToNotify = new List<User.UserDtos.UserResponseDto>();

          // Agregar owner si no es quien agregó el item
          if (listDetails.Owner != null && listDetails.Owner.UId != userUid)
          {
            usersToNotify.Add(listDetails.Owner);
          }

          // Agregar colaboradores (excepto quien agregó el item)
          usersToNotify.AddRange(listDetails.Collaborators.Where(c => c.UId != userUid));

          // Enviar notificaciones
          var notificationRequests = new List<NotificationRequest>();

          foreach (var user in usersToNotify)
          {
            // Verificar si el usuario quiere recibir notificaciones de items agregados
            var shouldReceiveNotification = await _settingService.GetItemAddedNotificationSettingAsync(user.UId);

            if (shouldReceiveNotification && !string.IsNullOrEmpty(user.FcmToken))
            {
              notificationRequests.Add(new NotificationRequest
              {
                FcmToken = user.FcmToken,
                Title = "📝 Nuevo item agregado",
                Body = $"{addedByUser?.DisplayName ?? "Alguien"} agregó '{item.Title}' a '{listDetails.Title}'",
                ClickAction = "OPEN_LIST",
                Data = new Dictionary<string, string>
                {
                  ["type"] = "added_item",
                  ["listId"] = item.ListId.ToString(),
                }
              });
            }
          }

          // Enviar notificaciones si hay usuarios para notificar
          if (notificationRequests.Any())
          {
            try
            {
              await _firebaseService.SendMultipleNotificationsAsync(notificationRequests);
            }
            catch (Exception notificationEx)
            {
              // Log el error pero no fallar la creación del item
              Console.WriteLine($"Error enviando notificaciones de item agregado: {notificationEx.Message}");
            }
          }
        }
        return Ok(item);
      }
      catch (ArgumentException ex)
      {
        return NotFound(new { message = ex.Message });
      }
    }

    // Eliminar una lista (solo el owner puede hacerlo)
    [HttpDelete("{listId}")]
    public async Task<IActionResult> DeleteList(int listId)
    {
      var userUid = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (string.IsNullOrEmpty(userUid))
        return Unauthorized();

      try
      {
        await _listService.DeleteListAsync(listId, userUid);
        return NoContent();
      }
      catch (UnauthorizedAccessException ex)
      {
        return Forbid(ex.Message);
      }
      catch (KeyNotFoundException ex)
      {
        return NotFound(new { message = ex.Message });
      }
    }

    // Eliminar un colaborador de una lista (solo el owner puede hacerlo)
    [HttpDelete("{listId}/collaborator/{collaboratorUid}")]
    public async Task<IActionResult> RemoveCollaborator(int listId, string collaboratorUid)
    {
      var userUid = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (string.IsNullOrEmpty(userUid))
        return Unauthorized();

      try
      {
        await _listService.RemoveCollaboratorAsync(listId, collaboratorUid, userUid);
        return Ok(new { message = "Colaborador eliminado exitosamente de la lista." });
      }
      catch (UnauthorizedAccessException ex)
      {
        return Forbid(ex.Message);
      }
      catch (KeyNotFoundException ex)
      {
        return NotFound(new { message = ex.Message });
      }
      catch (InvalidOperationException ex)
      {
        return BadRequest(new { message = ex.Message });
      }
    }

    // Verificar si el usuario autenticado es el owner de una lista
    [HttpGet("{listId}/is-owner")]
    public async Task<IActionResult> IsUserOwnerOfList(int listId)
    {
      var userUid = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (string.IsNullOrEmpty(userUid))
        return Unauthorized();

      try
      {
        var isOwner = await _listService.IsUserOwnerOfListAsync(listId, userUid);
        return Ok(new { isOwner });
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
      }
    }
  }
}
