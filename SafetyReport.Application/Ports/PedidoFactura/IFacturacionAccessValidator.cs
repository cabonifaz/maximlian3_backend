using SafetyReport.Models;

namespace SafetyReport.Application.Ports.PedidoFactura
{
    public interface IFacturacionAccessValidator
    {
        Task<Respuesta> ValidarAccesoFacturacionAsync(UsuarioGeneral usuarioLogueado, string razon);
    }
}
