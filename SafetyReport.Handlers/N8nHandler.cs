using SafetyReport.DAO;
using SafetyReport.Models;

namespace SafetyReport.Handlers
{
    public class N8nHandler
    {
        private readonly N8nDAO _dao;

        public N8nHandler(N8nDAO dao)
        {
            _dao = dao;
        }

        public async Task<Respuesta> ObtenerClienteAsync(UsuarioGeneral usuarioLogueado, string? emailBusqueda)
        {
            try
            {
                return await _dao.ObtenerClienteAsync(usuarioLogueado, emailBusqueda);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<N8nClienteConsulta>()
                };
            }
        }
    }
}
