using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;

namespace listly.Features.Firebase
{
  public class FirebaseService
  {
    private readonly FirebaseMessaging _messaging;

    public FirebaseService()
    {
      if (FirebaseApp.DefaultInstance == null)
      {
        FirebaseApp.Create(new AppOptions()
        {
          Credential = GoogleCredential.FromFile(Path.Combine(Directory.GetCurrentDirectory(), "android---listly-firebase-adminsdk-fbsvc-8a3e4e3a98.json"))
        });
      }
      _messaging = FirebaseMessaging.DefaultInstance;
    }

    // Enviar una notificación push a un solo dispositivo
    public async Task<string> SendNotificationAsync(NotificationRequest request)
    {
      if (string.IsNullOrEmpty(request.FcmToken))
      {
        throw new ArgumentException("El token FCM es obligatorio");
      }

      var messageData = new Dictionary<string, string>
      {
        ["title"] = request.Title ?? "🚨 Notificación",
        ["body"] = request.Body ?? "Contenido importante",
        ["click_action"] = request.ClickAction ?? "OPEN_MAIN_ACTIVITY"
      };

      // Agregar datos adicionales si se proporcionan
      if (request.Data != null)
      {
        foreach (var item in request.Data)
        {
          messageData[item.Key] = item.Value;
        }
      }

      var message = new Message()
      {
        Token = request.FcmToken,
        Data = messageData,
        Android = new AndroidConfig()
        {
          Priority = Priority.High,
          TimeToLive = TimeSpan.FromHours(1)
        }
      };

      try
      {
        var response = await _messaging.SendAsync(message);
        return response;
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException($"Error enviando notificación: {ex.Message}", ex);
      }
    }

    // Enviar notificaciones push a múltiples dispositivos
    public async Task<BatchResponse> SendMultipleNotificationsAsync(List<NotificationRequest> requests)
    {
      var messages = new List<Message>();

      foreach (var request in requests)
      {
        if (string.IsNullOrEmpty(request.FcmToken)) continue;

        var messageData = new Dictionary<string, string>
        {
          ["title"] = request.Title ?? "🚨 Notificación",
          ["body"] = request.Body ?? "Contenido importante",
          ["click_action"] = request.ClickAction ?? "OPEN_MAIN_ACTIVITY"
        };

        // Agregar datos adicionales si se proporcionan
        if (request.Data != null)
        {
          foreach (var item in request.Data)
          {
            messageData[item.Key] = item.Value;
          }
        }

        var message = new Message()
        {
          Token = request.FcmToken,
          Data = messageData,
          Android = new AndroidConfig()
          {
            Priority = Priority.High,
            TimeToLive = TimeSpan.FromHours(1)
          }
        };

        messages.Add(message);
      }

      if (messages.Count == 0)
      {
        throw new ArgumentException("No hay tokens FCM válidos para enviar notificaciones");
      }

      try
      {
        var response = await _messaging.SendEachAsync(messages);
        return response;
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException($"Error enviando notificaciones múltiples: {ex.Message}", ex);
      }
    }
  }
}

