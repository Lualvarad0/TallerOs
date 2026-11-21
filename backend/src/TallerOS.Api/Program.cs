using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using TallerOS.Api.Autenticacion;
using TallerOS.Api.Middleware;
using TallerOS.Application;
using TallerOS.Application.Comun;
using TallerOS.Infrastructure;
using TallerOS.Infrastructure.Seguridad;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Converter global: todo enum serializa como texto por defecto, para no depender de agregar
// [JsonConverter(typeof(JsonStringEnumConverter))] a mano en cada enum nuevo (varios, como
// TipoAdjunto/EstadoEmpresa/EstadoSuscripcion, quedaron sin él y serializaban como número).
builder.Services.AddControllers().AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddApplication();

// La API siempre se conecta con el rol de aplicación ("App"): sin DDL, sin BYPASSRLS.
builder.Services.AddInfrastructure(builder.Configuration, connectionStringName: "App");

// Pisa el ITenantContext por defecto (sin empresa) que registra AddInfrastructure: una vez
// autenticado, la empresa/privilegios de la sesión vienen de los claims del JWT, no de un default.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantContext, HttpContextTenantContext>();

var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()
    ?? throw new InvalidOperationException("Falta la sección Jwt en la configuración.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),
        };
    });

builder.Services.AddAuthorizationBuilder()
    // La mayoría de los endpoints: token de sesión (empresa y privilegios ya resueltos).
    .AddPolicy("Sesion", p => p.RequireClaim(JwtClaims.TipoToken, JwtClaims.TipoTokenSesion))
    // Solo /auth/seleccionar-empresa: token pre-auth (usuario con varias empresas, aún eligiendo).
    .AddPolicy("PreAuth", p => p.RequireClaim(JwtClaims.TipoToken, JwtClaims.TipoTokenPreAuth));

// Policies "permiso:{codigo}" generadas al vuelo (ver PermisoPolicyProvider): un endpoint nuevo que
// exige un permiso puntual no necesita registrar nada aquí.
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermisoPolicyProvider>();
builder.Services.AddSingleton<IAuthorizationHandler, PermisoAuthorizationHandler>();

const string PoliticaCorsFrontendDev = "FrontendDev";
builder.Services.AddCors(options =>
{
    // Solo para el frontend en desarrollo (apps/taller con `npm run dev`). En producción cada
    // dominio del frontend se agregará explícitamente, nunca con AllowAnyOrigin.
    options.AddPolicy(PoliticaCorsFrontendDev, p => p
        .WithOrigins("http://localhost:3000")
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<ManejoDeExcepcionesMiddleware>();

app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseCors(PoliticaCorsFrontendDev);
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Hace pública la clase Program que el compilador genera para top-level statements: sin esto,
// WebApplicationFactory<Program> (pruebas de integración por HTTP real, ver Api.Tests) no puede
// referenciarla desde otro ensamblado, porque por defecto es internal.
public partial class Program;
