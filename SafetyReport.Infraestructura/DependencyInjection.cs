using Amazon.BedrockRuntime;
using Amazon.S3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SafetyReport.Application.Puertos.Asignacion;
using SafetyReport.Application.Puertos.Banco;
using SafetyReport.Application.Puertos.Cliente;
using SafetyReport.Application.Puertos.ClienteContacto;
using SafetyReport.Application.Puertos.Compania;
using SafetyReport.Application.Puertos.DirectorioEjecutivo;
using SafetyReport.Application.Puertos.Informe;
using SafetyReport.Application.Puertos.InformeArchivo;
using SafetyReport.Application.Puertos.InformeLocalImagen;
using SafetyReport.Application.Puertos.InformeObservacion;
using SafetyReport.Application.Puertos.Login;
using SafetyReport.Application.Puertos.Pedido;
using SafetyReport.Application.Puertos.PedidoArchivo;
using SafetyReport.Application.Puertos.PedidoFactura;
using SafetyReport.Application.Puertos.PedidoFacturaLinea;
using SafetyReport.Application.Puertos.Almacenamiento;
using SafetyReport.Application.Puertos.TablaMaestra;
using SafetyReport.Application.Puertos.Tarifario;
using SafetyReport.Application.Puertos.Usuario;
using SafetyReport.Infrastructure.Automatizacion;
using SafetyReport.Infrastructure.GeneracionDocumentos;
using SafetyReport.Infrastructure.Correo;
using SafetyReport.Infrastructure.Exportacion;
using SafetyReport.Infrastructure.Facturacion;
using SafetyReport.Infrastructure.Persistencia;
using SafetyReport.Infrastructure.Seguridad;
using SafetyReport.Infrastructure.Almacenamiento;
using SafetyReport.Infrastructure.Traduccion;

namespace SafetyReport.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSafetyReportInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IPedidoPrefacturaExcelExporter, PedidoPrefacturaExcelExporter>();
        services.AddScoped<ICompaniaNoticiasDetalleExcelExporter, CompaniaNoticiasDetalleExcelExporter>();

        services.AddScoped<DocxGeneratorService>();
        services.AddScoped<IInformeDocxGenerator, DocxGeneratorService>();
        services.AddScoped<PdfGeneratorService>();
        services.AddScoped<IInformePdfGenerator, PdfGeneratorService>();

        var awsConfig = AddAwsConfig(services, configuration);
        AddSeguridad(services, configuration);
        AddPersistencia(services, configuration);
        AddStorage(services, awsConfig);
        AddTranslation(services, configuration, awsConfig);
        AddAutomation(services, configuration);
        AddEmail(services, configuration);
        AddFacturacionElectronica(services, configuration);

        return services;
    }

    private static AwsConfig AddAwsConfig(IServiceCollection services, IConfiguration configuration)
    {
        var awsConfig = configuration.GetSection("AWS").Get<AwsConfig>()
            ?? throw new Exception("Falta configuración AWS");

        if (string.IsNullOrWhiteSpace(awsConfig.Region))
            throw new Exception("Falta configuración AWS:Region");

        if (string.IsNullOrWhiteSpace(awsConfig.BucketName))
            throw new Exception("Falta configuración AWS:BucketName");

        if (string.IsNullOrWhiteSpace(awsConfig.AccessKey) || string.IsNullOrWhiteSpace(awsConfig.SecretKey))
            throw new Exception("Falta configuración AWS:AccessKey o AWS:SecretKey");

        services.AddSingleton(awsConfig);
        return awsConfig;
    }

    private static void AddSeguridad(IServiceCollection services, IConfiguration configuration)
    {
        var cognitoConfig = configuration.GetSection("Cognito").Get<CognitoConfig>()
            ?? throw new Exception("Falta configuración Cognito");

        if (string.IsNullOrWhiteSpace(cognitoConfig.UserPoolId))
            throw new Exception("Falta configuración Cognito:UserPoolId");

        if (string.IsNullOrWhiteSpace(cognitoConfig.ClientIdFrontend)
            || string.IsNullOrWhiteSpace(cognitoConfig.ClientIdBackend)
            || string.IsNullOrWhiteSpace(cognitoConfig.ClientIdN8n))
            throw new Exception("Falta configuración de clientes Cognito");

        services.AddSingleton(cognitoConfig);
        services.AddScoped<IUsuarioIdentityProvider, CognitoUsuarioIdentityProvider>();
        services.AddScoped<CognitoTokenValidator>();
        services.AddScoped<ITokenValidator, CognitoTokenValidator>();
    }

    private static void AddPersistencia(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new Exception("Falta ConnectionStrings:DefaultConnection");

        services.AddSingleton(new DbConfig(connectionString));

        services.AddScoped<LoginRepositorioSql>();
        services.AddScoped<ILoginRepository, LoginRepositorioSql>();
        services.AddScoped<UsuarioRepositorioSql>();
        services.AddScoped<IUsuarioRepository, UsuarioRepositorioSql>();
        services.AddScoped<TablaMaestraRepositorioSql>();
        services.AddScoped<ITablaMaestraRepository, TablaMaestraRepositorioSql>();
        services.AddScoped<ClienteRepositorioSql>();
        services.AddScoped<IClienteRepository, ClienteRepositorioSql>();
        services.AddScoped<TarifarioRepositorioSql>();
        services.AddScoped<ITarifarioRepository, TarifarioRepositorioSql>();
        services.AddScoped<ClienteContactoRepositorioSql>();
        services.AddScoped<IClienteContactoRepository, ClienteContactoRepositorioSql>();
        services.AddScoped<PedidoRepositorioSql>();
        services.AddScoped<IPedidoRepository, PedidoRepositorioSql>();
        services.AddScoped<PedidoFacturaRepositorioSql>();
        services.AddScoped<IPedidoFacturaRepository, PedidoFacturaRepositorioSql>();
        services.AddScoped<IFacturacionAccessValidator, PedidoFacturaRepositorioSql>();
        services.AddScoped<PedidoFacturaLineaRepositorioSql>();
        services.AddScoped<IPedidoFacturaLineaRepository, PedidoFacturaLineaRepositorioSql>();
        services.AddScoped<AsignacionRepositorioSql>();
        services.AddScoped<IAsignacionRepository, AsignacionRepositorioSql>();
        services.AddScoped<InformeRepositorioSql>();
        services.AddScoped<IInformeRepository, InformeRepositorioSql>();
        services.AddScoped<IInformeDraftRepository, InformeRepositorioSql>();
        services.AddScoped<InformeObservacionRepositorioSql>();
        services.AddScoped<IInformeObservacionRepository, InformeObservacionRepositorioSql>();
        services.AddScoped<InformeLocalImagenRepositorioSql>();
        services.AddScoped<IInformeLocalImagenRepository, InformeLocalImagenRepositorioSql>();
        services.AddScoped<InformeArchivoRepositorioSql>();
        services.AddScoped<IInformeArchivoRepository, InformeArchivoRepositorioSql>();
        services.AddScoped<BancoRepositorioSql>();
        services.AddScoped<IBancoRepository, BancoRepositorioSql>();
        services.AddScoped<CompaniaRepositorioSql>();
        services.AddScoped<ICompaniaRepository, CompaniaRepositorioSql>();
        services.AddScoped<DirectorioEjecutivoRepositorioSql>();
        services.AddScoped<IDirectorioEjecutivoRepository, DirectorioEjecutivoRepositorioSql>();
        services.AddScoped<PedidoArchivoRepositorioSql>();
        services.AddScoped<IPedidoArchivoRepository, PedidoArchivoRepositorioSql>();
    }

    private static void AddStorage(IServiceCollection services, AwsConfig awsConfig)
    {
        services.AddSingleton<IAmazonS3>(sp =>
        {
            var regionEndpoint = Amazon.RegionEndpoint.GetBySystemName(awsConfig.Region);
            var credenciales = new Amazon.Runtime.BasicAWSCredentials(awsConfig.AccessKey, awsConfig.SecretKey);
            return new AmazonS3Client(credenciales, regionEndpoint);
        });

        services.AddSingleton<S3UploadService>();
        services.AddSingleton<IPedidoArchivoStorage>(sp => sp.GetRequiredService<S3UploadService>());
        services.AddSingleton<IInformeLocalImagenStorage>(sp => sp.GetRequiredService<S3UploadService>());
        services.AddSingleton<IInformeArchivoStorage>(sp => sp.GetRequiredService<S3UploadService>());
        services.AddSingleton<ICompaniaNoticiaStorage>(sp => sp.GetRequiredService<S3UploadService>());
        services.AddSingleton<IInformeStorage>(sp => sp.GetRequiredService<S3UploadService>());
        services.AddSingleton<ILogStorage>(sp => sp.GetRequiredService<S3UploadService>());
        services.AddHttpClient<IArchivoRemotoDownloader, ArchivoRemotoDownloader>();
    }

    private static void AddTranslation(IServiceCollection services, IConfiguration configuration, AwsConfig awsConfig)
    {
        services.AddSingleton<IAmazonBedrockRuntime>(sp =>
        {
            var regionEndpoint = Amazon.RegionEndpoint.GetBySystemName(awsConfig.Region);
            var credenciales = new Amazon.Runtime.BasicAWSCredentials(awsConfig.AccessKey, awsConfig.SecretKey);
            return new AmazonBedrockRuntimeClient(credenciales, regionEndpoint);
        });
        services.AddSingleton<BedrockService>();

        var bedrockTranslationConfig = configuration.GetSection("BedrockTranslation").Get<BedrockTranslationConfig>()
            ?? throw new Exception("Falta configuración BedrockTranslation");

        if (string.IsNullOrWhiteSpace(bedrockTranslationConfig.TablaMaestra))
            throw new Exception("Falta configuración BedrockTranslation:TablaMaestra");

        if (string.IsNullOrWhiteSpace(bedrockTranslationConfig.Informe))
            throw new Exception("Falta configuración BedrockTranslation:Informe");

        services.AddSingleton(bedrockTranslationConfig);
        services.AddSingleton(sp =>
        {
            var bedrock = sp.GetRequiredService<BedrockService>();
            var config = sp.GetRequiredService<BedrockTranslationConfig>();
            return new BedrockTranslationService(bedrock, config.TablaMaestra);
        });
        services.AddSingleton<ITablaMaestraTranslator>(sp =>
            sp.GetRequiredService<BedrockTranslationService>());
        services.AddSingleton(sp =>
        {
            var bedrock = sp.GetRequiredService<BedrockService>();
            var config = sp.GetRequiredService<BedrockTranslationConfig>();
            return new BedrockInformeTranslationService(bedrock, config.Informe);
        });
        services.AddSingleton<IInformeTranslator>(sp =>
            sp.GetRequiredService<BedrockInformeTranslationService>());
    }

    private static void AddAutomation(IServiceCollection services, IConfiguration configuration)
    {
        var n8nConfig = configuration.GetSection("N8n").Get<N8nConfig>()
            ?? throw new Exception("Falta configuración N8n");

        if (string.IsNullOrWhiteSpace(n8nConfig.Secret))
            throw new Exception("Falta configuración N8n:Secret");

        if (string.IsNullOrWhiteSpace(n8nConfig.WebhookObtenerCampos))
            throw new Exception("Falta configuración N8n:WebhookObtenerCampos");

        services.AddSingleton(n8nConfig);
        services.AddHttpClient<N8nService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(300);
        });
        services.AddScoped<IInformeAutomationGateway>(sp =>
            sp.GetRequiredService<N8nService>());
    }

    private static void AddEmail(IServiceCollection services, IConfiguration configuration)
    {
        // Solo el ambiente "Production" real (buzon organizacional) usa auth app-only.
        // Cualquier otro nombre de ambiente (Development, Staging/PreProd, etc.) envia
        // desde la cuenta Hotmail de pruebas via auth delegada.
        var isProduction = string.Equals(configuration["Environment"], "Production", StringComparison.OrdinalIgnoreCase);
        if (!isProduction)
        {
            var devConfig = configuration.GetSection("Email:Dev").Get<EmailDevConfig>() ?? new EmailDevConfig();

            if (string.IsNullOrWhiteSpace(devConfig.ClientId) ||
                string.IsNullOrWhiteSpace(devConfig.Tenant) ||
                string.IsNullOrWhiteSpace(devConfig.TokenCacheS3Key))
            {
                services.AddSingleton<IInformeEmailSender, EmailServiceNoop>();
                return;
            }

            services.AddSingleton(devConfig);
            services.AddSingleton<IInformeEmailSender, EmailServiceDev>();
            return;
        }

        var prodConfig = configuration.GetSection("Email:Prod").Get<EmailProdConfig>() ?? new EmailProdConfig();

        if (string.IsNullOrWhiteSpace(prodConfig.TenantId) ||
            string.IsNullOrWhiteSpace(prodConfig.ClientId) ||
            string.IsNullOrWhiteSpace(prodConfig.ClientSecret) ||
            string.IsNullOrWhiteSpace(prodConfig.SenderMailbox))
        {
            services.AddSingleton<IInformeEmailSender, EmailServiceNoop>();
            return;
        }

        services.AddSingleton(prodConfig);
        services.AddSingleton<IInformeEmailSender, EmailServiceProd>();
    }

    private static void AddFacturacionElectronica(IServiceCollection services, IConfiguration configuration)
    {
        var facturacionElectronicaConfig = configuration.GetSection("FacturacionElectronica").Get<FacturacionElectronicaConfig>()
            ?? throw new Exception("Falta configuración FacturacionElectronica");

        if (string.IsNullOrWhiteSpace(facturacionElectronicaConfig.BaseUrl))
            throw new Exception("Falta configuración FacturacionElectronica:BaseUrl");

        if (string.IsNullOrWhiteSpace(facturacionElectronicaConfig.ApiKey))
            throw new Exception("Falta configuración FacturacionElectronica:ApiKey");

        services.AddSingleton(facturacionElectronicaConfig);
        services.AddHttpClient<FacturacionElectronicaService>();
        services.AddScoped<IFacturacionElectronicaGateway>(sp =>
            sp.GetRequiredService<FacturacionElectronicaService>());
    }
}
