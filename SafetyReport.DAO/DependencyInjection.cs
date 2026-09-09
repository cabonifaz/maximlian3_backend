using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SafetyReport.DAO.Persistencia;
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
using SafetyReport.Application.Puertos.TablaMaestra;
using SafetyReport.Application.Puertos.Tarifario;
using SafetyReport.Application.Puertos.Usuario;
using SafetyReport.Models;

namespace SafetyReport.DAO;

public static class DependencyInjection
{
    public static IServiceCollection AddSafetyReportDao(
        this IServiceCollection services,
        IConfiguration configuration)
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

        return services;
    }
}
