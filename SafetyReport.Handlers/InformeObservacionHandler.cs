using SafetyReport.DAO;
using SafetyReport.Models;

namespace SafetyReport.Handlers
{
    public class InformeObservacionHandler
    {
        private readonly InformeObservacionDAO _dao;

        public InformeObservacionHandler(InformeObservacionDAO dao)
        {
            _dao = dao;
        }

        public async Task<Respuesta> ListarObservacionesAsync(UsuarioGeneral usuarioLogueado, InformeObservacionListarRequest request)
        {
            try
            {
                return await _dao.ListarObservacionesAsync(usuarioLogueado, request.IdPedido);
            }
            catch (Exception)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new List<InformeObservacionConsulta>() };
            }
        }

        public async Task<Respuesta> InsertarObservacionesLoteAsync(UsuarioGeneral usuarioLogueado, InformeObservacionInsertarRequest request)
        {
            try
            {
                return await _dao.InsertarObservacionesLoteAsync(usuarioLogueado, request.IdInforme, request.IdPedido, request.Observaciones);
            }
            catch (Exception)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new List<object>() };
            }
        }

        public async Task<Respuesta> EditarObservacionAsync(UsuarioGeneral usuarioLogueado, InformeObservacionEditarRequest request)
        {
            try
            {
                return await _dao.EditarObservacionAsync(usuarioLogueado, request);
            }
            catch (Exception)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new List<object>() };
            }
        }

        public async Task<Respuesta> EliminarObservacionAsync(UsuarioGeneral usuarioLogueado, InformeObservacionIdRequest request)
        {
            try
            {
                return await _dao.EliminarObservacionAsync(usuarioLogueado, request.IdInformeObservacion);
            }
            catch (Exception)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new List<object>() };
            }
        }
    }
}
