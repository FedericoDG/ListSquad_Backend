# 📱 List Squad API – .NET 9 Web API

API REST para gestionar listas colaborativas, items, invitaciones y notificaciones push.
Desarrollado en ASP.NET Core Web API (.NET 9) con base de datos MySQL y Entity Framework Core.
Diseñado para ser consumido desde una aplicación móvil.

## Arquitectura

Features Implementadas

- Autenticación JWT - Login seguro para usuarios
- Gestión de Usuarios - Perfil y datos personales
- Listas - Creación y gestión de listas de squads
- Items - Gestión completa de items en listas (CRUD con notificaciones)
- Invitaciones - Sistema de invitaciones para unirse a listas
- Suscripciones - Gestión de pagos y suscripciones
- Configuraciones - Ajustes de usuario
- Notificaciones Push - Integración con Firebase para notificaciones en tiempo real

## Endpoints

```
POST   /api/auth/login                        # Login del usuario

GET    /api/users/me                          # Mi perfil como usuario
PUT    /api/users/me                          # Actualizar mis datos

GET    /api/lists                             # Listar mis listas
GET    /api/lists/{id}                        # Obtener lista por ID
POST   /api/lists                             # Crear una nueva lista
PUT    /api/lists/{id}                        # Actualizar lista
DELETE /api/lists/{id}                        # Eliminar lista

GET    /api/lists/{listId}/items              # Listar items de una lista
GET    /api/items/{id}                        # Obtener item por ID
POST   /api/lists/{listId}/items              # Agregar item a lista
PUT    /api/items/{id}                        # Actualizar item
PATCH  /api/items/{id}/toggle-completed      # Cambiar estado completado de item
DELETE /api/items/{id}                        # Eliminar item

GET    /api/invitations                       # Listar mis invitaciones
POST   /api/invitations                       # Enviar invitación
PUT    /api/invitations/{id}/accept           # Aceptar invitación
PUT    /api/invitations/{id}/reject           # Rechazar invitación

GET    /api/settings                          # Obtener configuraciones
PUT    /api/settings                          # Actualizar configuraciones

GET    /api/subscriptions                     # Listar suscripciones
POST   /api/subscriptions                     # Crear suscripción
```

## Requisitos previos

Antes de comenzar, asegúrate de tener instalado:

- Git
- Docker Desktop
- SDK .NET 9

## Instalación y ejecución del proyecto

### 1️⃣ Clonar el repositorio

```bash
git clone https://github.com/FedericoDG/ListSquad_Backend
cd ListSquad_Backend
```

### 2️⃣ Levantar base de datos MySQL con Docker

En la raíz del proyecto encontrarás un archivo `docker-compose.yml`. Ejecuta:

```bash
docker compose up -d
```

Esto creará:

- MySQL en localhost:3306
- phpMyAdmin en http://localhost:8080

Credenciales por defecto (modificables en `docker-compose.yml`):

- Usuario: root
- Password: 1234
- Base de datos: listly

### 3️⃣ Configurar la conexión en `appsettings.Development.json`

Verifica que la cadena de conexión esté configurada. También incluye las configuraciones para Firebase, MercadoPago y la URL base de la API:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;port=3306;database=listly;user=root;password=1234"
  },
  "MercadoPago": {
    "AccessToken": "TUE_ACCESS_TOKEN",
    "PublicKey": "TU_PUBLIC_KEY"
  },
  "APIURL": {
    "BaseURL": "URL_DE_TU_API_AQUI"
  }
}
```

### 3.1️⃣ Configurar Firebase

El archivo `android---listly-firebase-adminsdk-fbsvc-8a3e4e3a98.json` contiene las credenciales de servicio de Firebase para enviar notificaciones push. Asegúrate de que este archivo esté presente en la raíz del proyecto, ya que es necesario para la integración con Firebase Cloud Messaging.

### 4️⃣ Restaurar dependencias y compilar

```bash
dotnet restore
dotnet build
```

### 5️⃣ Aplicar migraciones (incluye datos de ejemplo)

```bash
dotnet ef database update
```

Esto creará automáticamente:

- ✅ Estructura de tablas (Users, Lists, Items, Invitations, Subscriptions, Settings)
- ✅ Datos de prueba (seeders) para testing

### 6️⃣ Ejecutar la API

```bash
dotnet run
```

La API estará disponible en:

- HTTP: http://localhost:5119
