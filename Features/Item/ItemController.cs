using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using listly.Features.Firebase;
using listly.Features.Setting;
using listly.Features.List;

namespace listly.Features.Item
{
  [ApiController]
  [Route("api/items")]
  [Authorize]
  public class ItemController : ControllerBase
  {
    private readonly ItemService _itemService;
    private readonly FirebaseService _firebaseService;
    private readonly SettingService _settingService;
    private readonly ListService _listService;

    public ItemController(ItemService itemService, FirebaseService firebaseService, SettingService settingService, ListService listService)
    {
      _itemService = itemService;
      _firebaseService = firebaseService;
      _settingService = settingService;
      _listService = listService;
    }

    // Alternar el estado completado de un ítem
    [HttpPatch("{itemId}/toggle-completed")]
    public async Task<IActionResult> ToggleCompleted(int itemId)
    {
      var userUid = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (string.IsNullOrEmpty(userUid))
        return Unauthorized();

      try
      {
        var item = await _itemService.ToggleCompletedAsync(itemId, userUid);
        if (item == null)
          return NotFound(new { message = "Ítem no encontrado." });

        // Convertir a DTO de respuesta
        var responseDto = new ItemDtos.ItemResponseDto
        {
          ItemId = item.ItemId,
          ListId = item.ListId,
          Title = item.Title,
          Completed = item.Completed,
          Description = item.Description,
          Notes = item.Notes,
          CheckedBy = item.CheckedBy,
          Quantity = item.Quantity,
          Unit = item.Unit
        };

        // Obtener detalles de la lista para las notificaciones
        var listDetails = await _listService.GetListByIdAsync(item.ListId, userUid);
        if (listDetails != null)
        {
          // Obtener el nombre del usuario que cambió el estado
          var changedByUser = listDetails.Collaborators.FirstOrDefault(c => c.UId == userUid)
                             ?? listDetails.Owner;

          // Crear lista de usuarios a notificar (todos excepto quien hizo el cambio)
          var usersToNotify = new List<User.UserDtos.UserResponseDto>();

          // Agregar owner si no es quien hizo el cambio
          if (listDetails.Owner != null && listDetails.Owner.UId != userUid)
          {
            usersToNotify.Add(listDetails.Owner);
          }

          // Agregar colaboradores (excepto quien hizo el cambio)
          usersToNotify.AddRange(listDetails.Collaborators.Where(c => c.UId != userUid));

          // Enviar notificaciones
          var notificationRequests = new List<NotificationRequest>();

          foreach (var user in usersToNotify)
          {
            // Verificar si el usuario quiere recibir notificaciones de cambios de estado
            var shouldReceiveNotification = await _settingService.GetItemStatusChangedNotificationSettingAsync(user.UId);

            if (shouldReceiveNotification && !string.IsNullOrEmpty(user.FcmToken))
            {
              var statusIcon = item.Completed ? "✅" : "❌";
              var statusText = item.Completed ? "compró" : "marcó como no comprado";

              notificationRequests.Add(new NotificationRequest
              {
                FcmToken = user.FcmToken,
                Title = $"{statusIcon} Estado de item actualizado",
                Body = $"{changedByUser?.DisplayName ?? "Alguien"} {statusText} '{item.Title}' en '{listDetails.Title}'",
                ClickAction = "OPEN_LIST",
                Data = new Dictionary<string, string>
                {
                  ["type"] = "toggle_item",
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
              // Log el error pero no fallar el cambio de estado
              Console.WriteLine($"Error enviando notificaciones de cambio de estado: {notificationEx.Message}");
            }
          }
        }

        return Ok(responseDto);
      }
      catch (UnauthorizedAccessException ex)
      {
        return Forbid(ex.Message);
      }
      catch (Exception ex)
      {
        return BadRequest(new { message = ex.Message });
      }
    }

    // Obtener un ítem por ID
    [HttpGet("{itemId}")]
    public async Task<IActionResult> GetItemById(int itemId)
    {
      var userUid = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (string.IsNullOrEmpty(userUid))
        return Unauthorized();

      try
      {
        var item = await _itemService.GetItemByIdAsync(itemId);
        if (item == null)
          return NotFound(new { message = "Ítem no encontrado." });

        // Verificar que el usuario tenga acceso a la lista del ítem
        var listDetails = await _listService.GetListByIdAsync(item.ListId, userUid);
        if (listDetails == null)
          return Forbid("No tienes acceso a este ítem.");

        // Convertir a DTO de respuesta
        var responseDto = new ItemDtos.ItemResponseDto
        {
          ItemId = item.ItemId,
          ListId = item.ListId,
          Title = item.Title,
          Completed = item.Completed,
          Description = item.Description,
          Notes = item.Notes,
          CheckedBy = item.CheckedBy,
          Quantity = item.Quantity,
          Unit = item.Unit
        };

        return Ok(responseDto);
      }
      catch (Exception ex)
      {
        return BadRequest(new { message = ex.Message });
      }
    }

    // Actualizar un ítem
    [HttpPut("{itemId}")]
    public async Task<IActionResult> UpdateItem(int itemId, [FromBody] ItemDtos.ItemUpdateDto dto)
    {
      var userUid = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (string.IsNullOrEmpty(userUid))
        return Unauthorized();

      try
      {
        var item = await _itemService.UpdateItemAsync(itemId, dto, userUid);
        if (item == null)
          return NotFound(new { message = "Ítem no encontrado." });

        // Convertir a DTO de respuesta
        var responseDto = new ItemDtos.ItemResponseDto
        {
          ItemId = item.ItemId,
          ListId = item.ListId,
          Title = item.Title,
          Completed = item.Completed,
          Description = item.Description,
          Notes = item.Notes,
          CheckedBy = item.CheckedBy,
          Quantity = item.Quantity,
          Unit = item.Unit
        };

        // Obtener detalles de la lista para las notificaciones
        var listDetails = await _listService.GetListByIdAsync(item.ListId, userUid);
        if (listDetails != null)
        {
          // Obtener el nombre del usuario que editó el item
          var editedByUser = listDetails.Collaborators.FirstOrDefault(c => c.UId == userUid)
                            ?? listDetails.Owner;

          // Crear lista de usuarios a notificar (todos excepto quien editó)
          var usersToNotify = new List<User.UserDtos.UserResponseDto>();

          // Agregar owner si no es quien editó el item
          if (listDetails.Owner != null && listDetails.Owner.UId != userUid)
          {
            usersToNotify.Add(listDetails.Owner);
          }

          // Agregar colaboradores (excepto quien editó el item)
          usersToNotify.AddRange(listDetails.Collaborators.Where(c => c.UId != userUid));

          // Enviar notificaciones
          var notificationRequests = new List<NotificationRequest>();

          foreach (var user in usersToNotify)
          {
            // Verificar si el usuario quiere recibir notificaciones de cambios de estado
            var shouldReceiveNotification = await _settingService.GetItemStatusChangedNotificationSettingAsync(user.UId);

            if (shouldReceiveNotification && !string.IsNullOrEmpty(user.FcmToken))
            {
              notificationRequests.Add(new NotificationRequest
              {
                FcmToken = user.FcmToken,
                Title = "✏️ Item editado",
                Body = $"{editedByUser?.DisplayName ?? "Alguien"} editó '{item.Title}' en '{listDetails.Title}'",
                ClickAction = "OPEN_LIST",
                Data = new Dictionary<string, string>
                {
                  ["type"] = "updated_item",
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
              // Log el error pero no fallar la actualización
              Console.WriteLine($"Error enviando notificaciones de item editado: {notificationEx.Message}");
            }
          }
        }

        return Ok(responseDto);
      }
      catch (UnauthorizedAccessException ex)
      {
        return Forbid(ex.Message);
      }
      catch (Exception ex)
      {
        return BadRequest(new { message = ex.Message });
      }
    }

    // Eliminar un ítem
    [HttpDelete("{itemId}")]
    public async Task<IActionResult> DeleteItem(int itemId)
    {
      var userUid = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (string.IsNullOrEmpty(userUid))
        return Unauthorized();

      try
      {
        // Obtener información del item antes de eliminarlo para las notificaciones
        var itemToDelete = await _itemService.GetItemByIdAsync(itemId);
        if (itemToDelete == null)
          return NotFound(new { message = "Ítem no encontrado." });

        // Obtener detalles de la lista para las notificaciones
        var listDetails = await _listService.GetListByIdAsync(itemToDelete.ListId, userUid);

        var result = await _itemService.DeleteItemAsync(itemId, userUid);
        if (!result)
          return NotFound(new { message = "Ítem no encontrado." });

        // Enviar notificaciones después de eliminar exitosamente
        if (listDetails != null)
        {
          // Obtener el nombre del usuario que eliminó el item
          var deletedByUser = listDetails.Collaborators.FirstOrDefault(c => c.UId == userUid)
                             ?? listDetails.Owner;

          // Crear lista de usuarios a notificar (todos excepto quien eliminó)
          var usersToNotify = new List<User.UserDtos.UserResponseDto>();

          // Agregar owner si no es quien eliminó el item
          if (listDetails.Owner != null && listDetails.Owner.UId != userUid)
          {
            usersToNotify.Add(listDetails.Owner);
          }

          // Agregar colaboradores (excepto quien eliminó el item)
          usersToNotify.AddRange(listDetails.Collaborators.Where(c => c.UId != userUid));

          // Enviar notificaciones
          var notificationRequests = new List<NotificationRequest>();

          foreach (var user in usersToNotify)
          {
            // Verificar si el usuario quiere recibir notificaciones de items eliminados
            var shouldReceiveNotification = await _settingService.GetItemDeletedNotificationSettingAsync(user.UId);

            if (shouldReceiveNotification && !string.IsNullOrEmpty(user.FcmToken))
            {
              notificationRequests.Add(new NotificationRequest
              {
                FcmToken = user.FcmToken,
                Title = "🗑️ Item eliminado",
                Body = $"{deletedByUser?.DisplayName ?? "Alguien"} eliminó '{itemToDelete.Title}' de '{listDetails.Title}'",
                ClickAction = "OPEN_LIST",
                Data = new Dictionary<string, string>
                {
                  ["type"] = "deleted_item",
                  ["listId"] = itemToDelete.ListId.ToString(),
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
              // Log el error pero no fallar la eliminación
              Console.WriteLine($"Error enviando notificaciones de item eliminado: {notificationEx.Message}");
            }
          }
        }

        return Ok(new { message = "Ítem eliminado exitosamente." });
      }
      catch (UnauthorizedAccessException ex)
      {
        return Forbid(ex.Message);
      }
      catch (Exception ex)
      {
        return BadRequest(new { message = ex.Message });
      }
    }
  }
}
