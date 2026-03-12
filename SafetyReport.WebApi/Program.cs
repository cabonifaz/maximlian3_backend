using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using SafetyReport.DAO;
using SafetyReport.Handlers;
using SafetyReport.Models;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SafetyReport.WebApi",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Bearer {token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

var region = builder.Configuration["AWS:Region"];
var userPoolId = builder.Configuration["Cognito:UserPoolId"];
var clientId = builder.Configuration["Cognito:ClientId"];
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

var authority = $"https://cognito-idp.{region}.amazonaws.com/{userPoolId}";

Console.WriteLine($"AUTHORITY CONFIG: {authority}");
Console.WriteLine($"CLIENT ID CONFIG: {clientId}");

builder.Services.AddSingleton(new DbConfig(connectionString));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = authority;
        options.IncludeErrorDetails = true;
        options.SaveToken = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = authority,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                Console.WriteLine("TOKEN RECIBIDO EN HEADER");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("TOKEN VALIDADO POR FIRMA/ISSUER/LIFETIME");

                if (context.SecurityToken is JwtSecurityToken jwt)
                {
                    var tokenUse = jwt.Claims.FirstOrDefault(c => c.Type == "token_use")?.Value;
                    var clientIdClaim = jwt.Claims.FirstOrDefault(c => c.Type == "client_id")?.Value;
                    var username = jwt.Claims.FirstOrDefault(c => c.Type == "username")?.Value;

                    if (!string.Equals(tokenUse, "access", StringComparison.OrdinalIgnoreCase))
                    {
                        context.Fail("Solo se aceptan access tokens.");
                        return Task.CompletedTask;
                    }

                    if (!string.Equals(clientIdClaim, clientId, StringComparison.Ordinal))
                    {
                        context.Fail($"El client_id del token ({clientIdClaim}) no coincide con el configurado ({clientId}).");
                        return Task.CompletedTask;
                    }
                }
                else
                {
                    context.Fail("No se pudo interpretar el JWT.");
                }

                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("AUTH FAILED:");
                Console.WriteLine(context.Exception.ToString());
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Console.WriteLine($"CHALLENGE ERROR: {context.Error}");
                Console.WriteLine($"CHALLENGE DESC: {context.ErrorDescription}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<LoginDAO>();
builder.Services.AddScoped<LoginHandler>();
builder.Services.AddScoped<UsuarioDAO>();
builder.Services.AddScoped<UsuarioHandler>();
builder.Services.AddScoped<CognitoTokenValidator>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();