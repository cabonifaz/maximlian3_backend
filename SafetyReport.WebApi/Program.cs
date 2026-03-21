using Amazon.S3;
using Amazon.SecurityToken;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using SafetyReport.DAO;
using SafetyReport.Handlers;
using SafetyReport.Models;

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
var clientIdFrontend = builder.Configuration["Cognito:ClientIdFrontend"];
var clientIdBackend = builder.Configuration["Cognito:ClientIdBackend"];
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

var cognitoIssuer = $"https://cognito-idp.{region}.amazonaws.com/{userPoolId}";
var validClientIds = new[] { clientIdFrontend, clientIdBackend };

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

            var tokenUse = context.Principal?.FindFirst("token_use")?.Value;
            var audClaim = context.Principal?.FindFirst("aud")?.Value;

            if (!string.Equals(tokenUse, "id", StringComparison.OrdinalIgnoreCase))
            {
                context.Fail("Solo se aceptan id tokens.");
                return Task.CompletedTask;
            }

            if (!validClientIds.Contains(audClaim, StringComparer.Ordinal))
            {
                context.Fail($"El aud del token ({audClaim}) no está autorizado.");
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
builder.Services.AddScoped<MasterTableDAO>();
builder.Services.AddScoped<MasterTableHandler>();
builder.Services.AddScoped<ClienteDAO>();
builder.Services.AddScoped<ClienteHandler>();
builder.Services.AddScoped<TarifarioDAO>();
builder.Services.AddScoped<TarifarioHandler>();
builder.Services.AddScoped<ClienteContactoHandler>();
builder.Services.AddScoped<ClienteContactoDAO>();
builder.Services.AddScoped<PedidoHandler>();
builder.Services.AddScoped<PedidoDAO>();

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
    var credentials = new Amazon.Runtime.BasicAWSCredentials(awsAccessKey, awsSecretKey);
    return new AmazonS3Client(credentials, regionEndpoint);
});

builder.Services.AddSingleton<IAmazonSecurityTokenService>(sp =>
{
    var regionEndpoint = Amazon.RegionEndpoint.GetBySystemName(awsRegion);
    var credentials = new Amazon.Runtime.BasicAWSCredentials(awsAccessKey, awsSecretKey);
    return new AmazonSecurityTokenServiceClient(credentials, regionEndpoint);
});

builder.Services.AddSingleton<IS3UploadService, S3UploadService>();

builder.Services.AddScoped<PedidoArchivoHandler>();
builder.Services.AddScoped<PedidoArchivoDAO>();
builder.Services.AddScoped<CognitoTokenValidator>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("LocalDev");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();