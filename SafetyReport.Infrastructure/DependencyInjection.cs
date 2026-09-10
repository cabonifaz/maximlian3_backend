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
        AddTranslation(services, configuration);
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

        services.AddScoped<LoginDAO>();
        services.AddScoped<ILoginRepository, LoginDAO>();
        services.AddScoped<UsuarioDAO>();
        services.AddScoped<IUsuarioRepository, UsuarioDAO>();
        services.AddScoped<TablaMaestraDAO>();
        services.AddScoped<ITablaMaestraRepository, TablaMaestraDAO>();
        services.AddScoped<ClienteDAO>();
        services.AddScoped<IClienteRepository, ClienteDAO>();
        services.AddScoped<TarifarioDAO>();
        services.AddScoped<ITarifarioRepository, TarifarioDAO>();
        services.AddScoped<ClienteContactoDAO>();
        services.AddScoped<IClienteContactoRepository, ClienteContactoDAO>();
        services.AddScoped<PedidoDAO>();
        services.AddScoped<IPedidoRepository, PedidoDAO>();
        services.AddScoped<PedidoFacturaDAO>();
        services.AddScoped<IPedidoFacturaRepository, PedidoFacturaDAO>();
        services.AddScoped<IFacturacionAccessValidator, PedidoFacturaDAO>();
        services.AddScoped<PedidoFacturaLineaDAO>();
        services.AddScoped<IPedidoFacturaLineaRepository, PedidoFacturaLineaDAO>();
        services.AddScoped<AsignacionDAO>();
        services.AddScoped<IAsignacionRepository, AsignacionDAO>();
        services.AddScoped<InformeDAO>();
        services.AddScoped<IInformeRepository, InformeDAO>();
        services.AddScoped<IInformeDraftRepository, InformeDAO>();
        services.AddScoped<InformeObservacionDAO>();
        services.AddScoped<IInformeObservacionRepository, InformeObservacionDAO>();
        services.AddScoped<InformeLocalImagenDAO>();
        services.AddScoped<IInformeLocalImagenRepository, InformeLocalImagenDAO>();
        services.AddScoped<InformeArchivoDAO>();
        services.AddScoped<IInformeArchivoRepository, InformeArchivoDAO>();
        services.AddScoped<PlantillaDocumentoDAO>();
        services.AddScoped<BancoDAO>();
        services.AddScoped<IBancoRepository, BancoDAO>();
        services.AddScoped<CompaniaDAO>();
        services.AddScoped<ICompaniaRepository, CompaniaDAO>();
        services.AddScoped<DirectorioEjecutivoDAO>();
        services.AddScoped<IDirectorioEjecutivoRepository, DirectorioEjecutivoDAO>();
        services.AddScoped<PedidoArchivoDAO>();
        services.AddScoped<IPedidoArchivoRepository, PedidoArchivoDAO>();
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

    private static void AddTranslation(IServiceCollection services, IConfiguration configuration)
    {
        var awsRegion = configuration["AWS:Region"];
        var awsAccessKey = configuration["AWS:AccessKey"];
        var awsSecretKey = configuration["AWS:SecretKey"];

        services.AddSingleton<IAmazonBedrockRuntime>(sp =>
        {
            var regionEndpoint = Amazon.RegionEndpoint.GetBySystemName(awsRegion);
            var credenciales = new Amazon.Runtime.BasicAWSCredentials(awsAccessKey, awsSecretKey);
            return new AmazonBedrockRuntimeClient(credenciales, regionEndpoint);
        });
        services.AddSingleton<BedrockService>();

        var bedrockTranslationConfig = configuration.GetSection("BedrockTranslation").Get<BedrockTranslationConfig>()
            ?? throw new Exception("Falta configuración BedrockTranslation");
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
        var emailConfig = configuration.GetSection("Email").Get<EmailConfig>()
            ?? throw new Exception("Falta configuración Email");
        services.AddSingleton(emailConfig);
        services.AddSingleton<IInformeEmailSender, EmailService>();
    }

    private static void AddFacturacionElectronica(IServiceCollection services, IConfiguration configuration)
    {
        var facturacionElectronicaConfig = configuration.GetSection("FacturacionElectronica").Get<FacturacionElectronicaConfig>()
            ?? throw new Exception("Falta configuración FacturacionElectronica");
        services.AddSingleton(facturacionElectronicaConfig);
        services.AddHttpClient<FacturacionElectronicaService>();
        services.AddScoped<IFacturacionElectronicaGateway>(sp =>
            sp.GetRequiredService<FacturacionElectronicaService>());
    }
}
