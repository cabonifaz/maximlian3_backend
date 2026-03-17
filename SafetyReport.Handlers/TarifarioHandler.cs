using SafetyReport.DAO;
using SafetyReport.Models;

namespace SafetyReport.Handlers
{
    public class TarifarioHandler
    {
        private readonly TarifarioDAO _dao;

        public TarifarioHandler(TarifarioDAO dao)
        {
            _dao = dao;
        }

        public async Task<Respuesta> CrearAsync(UsuarioGeneral usuarioLogueado, TarifarioCrear request)
        {
            try
            {
                return await _dao.CrearAsync(usuarioLogueado, request);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 1,
                    Mensaje = ex.Message,
                    Result = new List<TarifarioCreado>()
                };
            }
        }

        public async Task<Respuesta> ListarAsync(UsuarioGeneral usuarioLogueado, TarifarioFiltro request)
        {
            try
            {
                return await _dao.ListarAsync(usuarioLogueado, request);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 1,
                    Mensaje = ex.Message,
                    Result = new TarifarioListaResult()
                };
            }
        }

        public async Task<Respuesta> ObtenerAsync(UsuarioGeneral usuarioLogueado, TarifarioIdRequest request)
        {
            try
            {
                return await _dao.ObtenerAsync(usuarioLogueado, request);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 1,
                    Mensaje = ex.Message,
                    Result = new List<TarifarioConsulta>()
                };
            }
        }

        public async Task<Respuesta> EditarAsync(UsuarioGeneral usuarioLogueado, TarifarioEditar request)
        {
            try
            {
                return await _dao.EditarAsync(usuarioLogueado, request);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 1,
                    Mensaje = ex.Message,
                    Result = new List<TarifarioCreado>()
                };
            }
        }

        public async Task<Respuesta> EliminarAsync(UsuarioGeneral usuarioLogueado, TarifarioIdRequest request)
        {
            try
            {
                return await _dao.EliminarAsync(usuarioLogueado, request);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 1,
                    Mensaje = ex.Message,
                    Result = new List<TarifarioEliminado>()
                };
            }
        }
    }
}