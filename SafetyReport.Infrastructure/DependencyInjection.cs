using Amazon.BedrockRuntime;
using Amazon.S3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SafetyReport.Application.Ports.Compania;
using SafetyReport.Application.Ports.Informe;
using SafetyReport.Application.Ports.Login;
using SafetyReport.Application.Ports.PedidoFactura;
using SafetyReport.Application.Ports.Storage;
using SafetyReport.Application.Ports.TablaMaestra;
using SafetyReport.Application.Ports.Usuario;
using SafetyReport.Infrastructure.Automation;
using SafetyReport.Infrastructure.DocumentGeneration;
using SafetyReport.Infrastructure.Email;
using SafetyReport.Infrastructure.Export;
using SafetyReport.Infrastructure.Facturacion;
using SafetyReport.Infrastructure.Identity;
using SafetyReport.Infrastructure.Storage;
using SafetyReport.Infrastructure.Translation;
using SafetyReport.Models;

namespace SafetyReport.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSafetyReportInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IUsuarioIdentityProvider, CognitoUsuarioIdentityProvider>();
        services.AddScoped<CognitoTokenValidator>();
        services.AddScoped<ITokenValidator, CognitoTokenValidator>();

        services.AddScoped<IPedidoPrefacturaExcelExporter, PedidoPrefacturaExcelExporter>();
        services.AddScoped<ICompaniaNoticiasDetalleExcelExporter, CompaniaNoticiasDetalleExcelExporter>();

        services.AddScoped<DocxGeneratorService>();
        services.AddScoped<IInformeDocxGenerator, DocxGeneratorService>();
        services.AddScoped<PdfGeneratorService>();
        services.AddScoped<IInformePdfGenerator, PdfGeneratorService>();

        AddStorage(services, configuration);
        AddTranslation(services, configuration);
        AddAutomation(services, configuration);
        AddEmail(services, configuration);
        AddFacturacionElectronica(services, configuration);

        return services;
    }

    private static void AddStorage(IServiceCollection services, IConfiguration configuration)
    {
        var awsRegion = configuration["AWS:Region"];
        var awsBucketName = configuration["AWS:BucketName"];
        var awsAccessKey = configuration["AWS:AccessKey"];
        var awsSecretKey = configuration["AWS:SecretKey"];

        if (string.IsNullOrWhiteSpace(awsRegion))
            throw new Exception("Falta configuración AWS:Region");

        if (string.IsNullOrWhiteSpace(awsBucketName))
            throw new Exception("Falta configuración AWS:BucketName");

        if (string.IsNullOrWhiteSpace(awsAccessKey) || string.IsNullOrWhiteSpace(awsSecretKey))
            throw new Exception("Falta configuración AWS:AccessKey o AWS:SecretKey");

        services.AddSingleton<IAmazonS3>(sp =>
        {
            var regionEndpoint = Amazon.RegionEndpoint.GetBySystemName(awsRegion);
            var credenciales = new Amazon.Runtime.BasicAWSCredentials(awsAccessKey, awsSecretKey);
            return new AmazonS3Client(credenciales, regionEndpoint);
        });

        services.AddSingleton<S3UploadService>();
        services.AddSingleton<IPedidoArchivoStorage>(sp => sp.GetRequiredService<S3UploadService>());
        services.AddSingleton<IInformeLocalImagenStorage>(sp => sp.GetRequiredService<S3UploadService>());
        services.AddSingleton<IInformeArchivoStorage>(sp => sp.GetRequiredService<S3UploadService>());
        services.AddSingleton<ICompaniaNoticiaStorage>(sp => sp.GetRequiredService<S3UploadService>());
        services.AddSingleton<IInformeStorage>(sp => sp.GetRequiredService<S3UploadService>());
        services.AddSingleton<ILogStorage>(sp => sp.GetRequiredService<S3UploadService>());
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
