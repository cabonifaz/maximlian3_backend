using Amazon.BedrockRuntime;
using Amazon.S3;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using NLog;
using NLog.Web;
using SafetyReport.DAO;
using SafetyReport.Handlers;
using SafetyReport.Models;
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
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

var cognitoIssuer = $"https://cognito-idp.{region}.amazonaws.com/{idPoolUsuarios}";
var validClientIds = new[] { clientIdFrontend, clientIdBackend, clientIdN8n };

Console.WriteLine($"AUTHORITY CONFIG: {cognitoIssuer}");
Console.WriteLine($"CLIENT ID FRONTEND: {clientIdFrontend}");
Console.WriteLine($"CLIENT ID BACKEND: {clientIdBackend}");

builder.Services.AddSingleton(new DbConfig(connectionString!));

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
              .AllowCredentials();
    });
});

builder.Services.AddScoped<LoginDAO>();
builder.Services.AddScoped<LoginHandler>();
builder.Services.AddScoped<UsuarioDAO>();
builder.Services.AddScoped<UsuarioHandler>();
builder.Services.AddScoped<TablaMaestraDAO>();
builder.Services.AddScoped<TablaMaestraHandler>();
builder.Services.AddScoped<FormatoDocumentoResolver>();
builder.Services.AddScoped<ClienteDAO>();
builder.Services.AddScoped<ClienteHandler>();
builder.Services.AddScoped<TarifarioDAO>();
builder.Services.AddScoped<TarifarioHandler>();
builder.Services.AddScoped<ClienteContactoHandler>();
builder.Services.AddScoped<ClienteContactoDAO>();
builder.Services.AddScoped<PedidoHandler>();
builder.Services.AddScoped<PedidoDAO>();
builder.Services.AddScoped<AsignacionHandler>();
builder.Services.AddScoped<AsignacionDAO>();
builder.Services.AddScoped<DocxGeneratorService>();
builder.Services.AddScoped<PdfGeneratorService>();
builder.Services.AddScoped<InformeHandler>();
builder.Services.AddScoped<InformeDAO>();
builder.Services.AddScoped<InformeObservacionHandler>();
builder.Services.AddScoped<InformeObservacionDAO>();
builder.Services.AddScoped<InformeLocalImagenHandler>();
builder.Services.AddScoped<InformeLocalImagenDAO>();
builder.Services.AddScoped<InformeArchivoHandler>();
builder.Services.AddScoped<InformeArchivoDAO>();
builder.Services.AddScoped<PlantillaDocumentoDAO>();
builder.Services.AddScoped<BancoHandler>();
builder.Services.AddScoped<BancoDAO>();
builder.Services.AddScoped<CompaniaHandler>();
builder.Services.AddScoped<CompaniaDAO>();
builder.Services.AddScoped<DirectorioEjecutivoHandler>();
builder.Services.AddScoped<DirectorioEjecutivoDAO>();

var awsRegion = builder.Configuration["AWS:Region"];
var awsBucketName = builder.Configuration["AWS:BucketName"];
var awsAccessKey = builder.Configuration["AWS:AccessKey"];
var awsSecretKey = builder.Configuration["AWS:SecretKey"];

if (string.IsNullOrWhiteSpace(awsRegion))
    throw new Exception("Falta configuración AWS:Region");

if (string.IsNullOrWhiteSpace(awsBucketName))
    throw new Exception("Falta configuración AWS:BucketName");

if (string.IsNullOrWhiteSpace(awsAccessKey) || string.IsNullOrWhiteSpace(awsSecretKey))
    throw new Exception("Falta configuración AWS:AccessKey o AWS:SecretKey");

builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var regionEndpoint = Amazon.RegionEndpoint.GetBySystemName(awsRegion);
    var credenciales = new Amazon.Runtime.BasicAWSCredentials(awsAccessKey, awsSecretKey);
    return new AmazonS3Client(credenciales, regionEndpoint);
});

builder.Services.AddSingleton<IS3UploadService, S3UploadService>();

builder.Services.AddSingleton<IAmazonBedrockRuntime>(sp =>
{
    var regionEndpoint = Amazon.RegionEndpoint.GetBySystemName(awsRegion);
    var credenciales = new Amazon.Runtime.BasicAWSCredentials(awsAccessKey, awsSecretKey);
    return new AmazonBedrockRuntimeClient(credenciales, regionEndpoint);
});
builder.Services.AddSingleton<BedrockService>();

var bedrockTranslationConfig = builder.Configuration.GetSection("BedrockTranslation").Get<BedrockTranslationConfig>()
    ?? throw new Exception("Falta configuración BedrockTranslation");
builder.Services.AddSingleton(bedrockTranslationConfig);
builder.Services.AddSingleton(sp =>
{
    var bedrock = sp.GetRequiredService<BedrockService>();
    var config = sp.GetRequiredService<BedrockTranslationConfig>();
    return new BedrockTranslationService(bedrock, config.TablaMaestra);
});
builder.Services.AddSingleton(sp =>
{
    var bedrock = sp.GetRequiredService<BedrockService>();
    var config = sp.GetRequiredService<BedrockTranslationConfig>();
    return new BedrockInformeTranslationService(bedrock, config.Informe);
});
builder.Services.AddScoped<InformeTranslationHandler>();

builder.Services.AddScoped<PedidoArchivoHandler>();
builder.Services.AddScoped<PedidoArchivoDAO>();
builder.Services.AddScoped<CognitoTokenValidator>();

var n8nConfig = builder.Configuration.GetSection("N8n").Get<N8nConfig>()
    ?? throw new Exception("Falta configuración N8n");
builder.Services.AddSingleton(n8nConfig);
builder.Services.AddHttpClient<N8nService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(300);
});

var emailConfig = builder.Configuration.GetSection("Email").Get<EmailConfig>()
    ?? throw new Exception("Falta configuración Email");
builder.Services.AddSingleton(emailConfig);
builder.Services.AddSingleton<IEmailService, EmailService>();

var app = builder.Build();

S3FallbackTarget.UploadService = app.Services.GetRequiredService<IS3UploadService>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("LocalDev");
app.UseRequestTimeouts();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

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
