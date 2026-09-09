using SafetyReport.Models;

namespace SafetyReport.Application.Puertos.PedidoFactura
{
    public interface IFacturacionAccessValidator
    {
        Task<Respuesta> ValidarAccesoFacturacionAsync(UsuarioGeneral usuarioLogueado, string razon);
    }
}
