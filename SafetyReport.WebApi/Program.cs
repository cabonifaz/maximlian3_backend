using Microsoft.AspNetCore.Authentication.JwtBearer;
using SafetyReport.Handlers;
using SafetyReport.DAO;
using SafetyReport.Models;

var builder = WebApplication.CreateBuilder(args);

// Controllers / Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Leer configuración
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var cognitoAuthority = $"https://cognito-idp.{builder.Configuration["AWS:Region"]}.amazonaws.com/{builder.Configuration["Cognito:UserPoolId"]}";
var cognitoAudience = builder.Configuration["Cognito:ClientId"];

// Registrar DbConfig para los DAO
builder.Services.AddSingleton(new DbConfig(connectionString));

// JWT Bearer con Cognito
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = cognitoAuthority;
        options.Audience = cognitoAudience;
    });

builder.Services.AddAuthorization();

// Inyección de dependencias
builder.Services.AddScoped<LoginDAO>();
builder.Services.AddScoped<LoginHandler>();
builder.Services.AddScoped<UsuarioHandler>();
builder.Services.AddScoped<UsuarioDAO>();

var app = builder.Build();


    app.UseSwagger();
    app.UseSwaggerUI();


    app.UseAuthentication();
    app.UseAuthorization();

app.MapControllers();

app.Run();