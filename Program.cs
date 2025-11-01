using System.Text;
using listly;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Agregar servicios de controladores
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
      options.JsonSerializerOptions.PropertyNamingPolicy = null;
      options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// EF Core con MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
  throw new InvalidOperationException("La cadena de conexión 'DefaultConnection' no fue encontrada.");
}

builder.Services.AddDbContext<ListlyDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Registrar servicios personalizados
builder.Services.AddScoped<listly.Features.Auth.JwtService>();
builder.Services.AddScoped<listly.Features.Auth.AuthServices>();
builder.Services.AddScoped<listly.Features.User.UserServices>();
builder.Services.AddScoped<listly.Features.List.ListService>();
builder.Services.AddScoped<listly.Features.Invitation.InvitationService>();
builder.Services.AddScoped<listly.Features.Item.ItemService>();
builder.Services.AddScoped<listly.Features.Setting.SettingService>();

// FirebaseService opcional - solo si las credenciales están disponibles
try
{
  builder.Services.AddScoped<listly.Features.Firebase.FirebaseService>();
}
catch (Exception ex)
{
  Console.WriteLine($"Firebase no disponible: {ex.Message}");
}

builder.Services.AddScoped<listly.Features.Subscription.SubscriptionService>();

// Configurar JWT Authentication
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

// Agregar autenticación JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
      options.TokenValidationParameters = new TokenValidationParameters
      {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey!)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
      };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configurar el middleware HTTP
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Agregar middleware de autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

// Mapear controladores
app.MapControllers();

app.Run();
