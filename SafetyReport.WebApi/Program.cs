using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using NLog;
using NLog.Web;
using SafetyReport.Application.Puertos.Almacenamiento;
using SafetyReport.Application;
using SafetyReport.Infrastructure;
using SafetyReport.WebApi.Filters;
using SafetyReport.WebApi.Helpers;
using SafetyReport.WebApi.Logging;

var nlogLogger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

try
{

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Host.UseNLog();

builder.Services.AddControllers(options =>
    options.Filters.Add<SanitizeErrorFilter>());
builder.Services.AddHealthChecks();
builder.Services.AddRequestTimeouts();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();

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
        Description = "Access token de Cognito: Bearer {access_token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });

    c.OperationFilter<SwaggerHeaderFilter>();
});

var region = builder.Configuration["AWS:Region"];
var idPoolUsuarios = builder.Configuration["Cognito:UserPoolId"];
var clientIdFrontend = builder.Configuration["Cognito:ClientIdFrontend"];
var clientIdBackend = builder.Configuration["Cognito:ClientIdBackend"];
var clientIdN8n = builder.Configuration["Cognito:ClientIdN8n"];

var cognitoIssuer = $"https://cognito-idp.{region}.amazonaws.com/{idPoolUsuarios}";
var validClientIds = new[] { clientIdFrontend, clientIdBackend, clientIdN8n };

Console.WriteLine($"AUTHORITY CONFIG: {cognitoIssuer}");
Console.WriteLine($"CLIENT ID FRONTEND: {clientIdFrontend}");
Console.WriteLine($"CLIENT ID BACKEND: {clientIdBackend}");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Authority = cognitoIssuer;
    options.MetadataAddress = $"{cognitoIssuer}/.well-known/openid-configuration";
    options.IncludeErrorDetails = true;
    options.SaveToken = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = cognitoIssuer,
        // ID Tokens de Cognito incluyen el claim "aud" con el client_id
        // La validación del aud se hace manualmente en OnTokenValidated
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        // Los access tokens de Cognito traen el username en el claim "username",
        // no en el claim estándar de .NET. Sin esto, HttpContext.User.Identity.Name
        // (usado por ${aspnet-user-identity} en nlog.config) siempre sale NULL.
        NameClaimType = "username"
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

            var tokenUse = context.Principal?.FindFirst("token_use")?.Value;
            var clientIdClaim = context.Principal?.FindFirst("client_id")?.Value;

            if (!string.Equals(tokenUse, "access", StringComparison.OrdinalIgnoreCase))
            {
                context.Fail("Solo se aceptan access tokens.");
                return Task.CompletedTask;
            }

            if (!validClientIds.Contains(clientIdClaim, StringComparer.Ordinal))
            {
                context.Fail($"El client_id del token ({clientIdClaim}) no está autorizado.");
                return Task.CompletedTask;
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

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("LocalDev", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              // Por default el navegador solo expone a JS un set fijo de response headers "safelisted"
              // (Content-Type, Content-Length, etc.) — Content-Disposition (nombre del archivo en las
              // descargas, p.ej. sireRvie/txt) queda invisible para el fetch/XHR del front si no se declara
              // acá explícitamente, aunque el servidor sí lo mande.
              .WithExposedHeaders("Content-Disposition");
    });
});

builder.Services.AddSafetyReportApplication(options =>
{
    options.S3ExpirationMinutes = int.TryParse(builder.Configuration["AWS:S3ExpirationTime"], out var exp) ? exp : 15;
    options.FrontendUrl = builder.Configuration.GetSection("Cors:AllowedOrigins").GetChildren().FirstOrDefault()?.Value
        ?? string.Empty;
});
builder.Services.AddSafetyReportInfrastructure(builder.Configuration);
builder.Services.AddHostedService<SafetyReport.WebApi.Workers.SincronizacionFacturacionWorker>();

var app = builder.Build();

S3FallbackTarget.UploadService = app.Services.GetRequiredService<ILogStorage>();

var nlogConfiguration = LogManager.Configuration
    ?? throw new InvalidOperationException("La configuracion de NLog no se cargo.");
var consoleTarget = nlogConfiguration.FindTargetByName("console");
if (consoleTarget is not null)
{
    // Reusa Logging:LogLevel:Default (convención estándar de .NET) en vez de una clave
    // propia. Los nombres de nivel de Microsoft.Extensions.Logging no coinciden 1:1 con
    // los de NLog (Information/Warning/Critical/None vs Info/Warn/Fatal/Off), de ahí el mapeo.
    var consoleMinLevel = (builder.Configuration["Logging:LogLevel:Default"] ?? "Warning") switch
    {
        "Trace" => NLog.LogLevel.Trace,
        "Debug" => NLog.LogLevel.Debug,
        "Information" => NLog.LogLevel.Info,
        "Warning" => NLog.LogLevel.Warn,
        "Error" => NLog.LogLevel.Error,
        "Critical" => NLog.LogLevel.Fatal,
        "None" => NLog.LogLevel.Off,
        _ => NLog.LogLevel.Warn
    };

    nlogConfiguration.RemoveRuleByName("console");
    nlogConfiguration.LoggingRules.Add(new NLog.Config.LoggingRule("*", consoleMinLevel, NLog.LogLevel.Fatal, consoleTarget)
    {
        RuleName = "console"
    });
    LogManager.ReconfigExistingLoggers();
}

var isDevelopment = string.Equals(builder.Configuration["Environment"], "Development", StringComparison.OrdinalIgnoreCase);
if (isDevelopment)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("LocalDev");
app.UseRequestTimeouts();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

}
catch (Exception ex)
{
    nlogLogger.Error(ex, "Aplicacion detenida por una excepcion no controlada durante el arranque");
    throw;
}
finally
{
    LogManager.Shutdown();
}
