using Microsoft.Extensions.DependencyInjection;
using SafetyReport.Application.Ports.Asignacion;
using SafetyReport.Application.Ports.Banco;
using SafetyReport.Application.Ports.Cliente;
using SafetyReport.Application.Ports.ClienteContacto;
using SafetyReport.Application.Ports.Compania;
using SafetyReport.Application.Ports.DirectorioEjecutivo;
using SafetyReport.Application.Ports.Informe;
using SafetyReport.Application.Ports.InformeArchivo;
using SafetyReport.Application.Ports.InformeLocalImagen;
using SafetyReport.Application.Ports.InformeObservacion;
using SafetyReport.Application.Ports.Login;
using SafetyReport.Application.Ports.Pedido;
using SafetyReport.Application.Ports.PedidoArchivo;
using SafetyReport.Application.Ports.PedidoFactura;
using SafetyReport.Application.Ports.PedidoFacturaLinea;
using SafetyReport.Application.Ports.TablaMaestra;
using SafetyReport.Application.Ports.Tarifario;
using SafetyReport.Application.Ports.Usuario;

namespace SafetyReport.DAO;

public static class DependencyInjection
{
    public static IServiceCollection AddSafetyReportDao(this IServiceCollection services)
    {
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

        return services;
    }
}
