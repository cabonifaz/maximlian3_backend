using Microsoft.Extensions.DependencyInjection;
using SafetyReport.Application.Comun;
using SafetyReport.Application.CasosDeUso;

namespace SafetyReport.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddSafetyReportApplication(
        this IServiceCollection services,
        Action<OpcionesAplicacion>? configureOptions = null)
    {
        var opciones = new OpcionesAplicacion();
        configureOptions?.Invoke(opciones);
        services.AddSingleton(opciones);

        services.AddScoped<AsignacionHandler>();
        services.AddScoped<BancoHandler>();
        services.AddScoped<ClienteContactoHandler>();
        services.AddScoped<ClienteHandler>();
        services.AddScoped<CompaniaHandler>();
        services.AddScoped<DirectorioEjecutivoHandler>();
        services.AddScoped<FormatoDocumentoResolver>();
        services.AddScoped<InformeArchivoHandler>();
        services.AddScoped<InformeHandler>();
        services.AddScoped<InformeLocalImagenHandler>();
        services.AddScoped<InformeObservacionHandler>();
        services.AddScoped<InformeTranslationHandler>();
        services.AddScoped<LoginHandler>();
        services.AddScoped<PedidoArchivoHandler>();
        services.AddScoped<PedidoFacturaHandler>();
        services.AddScoped<PedidoFacturaLineaHandler>();
        services.AddScoped<PedidoHandler>();
        services.AddScoped<TablaMaestraHandler>();
        services.AddScoped<TarifarioHandler>();
        services.AddScoped<UsuarioHandler>();
        services.AddScoped<VerificacionFacturaHandler>();

        return services;
    }
}
