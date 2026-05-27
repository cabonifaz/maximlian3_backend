using SafetyReport.DAO;
using SafetyReport.Models;

namespace SafetyReport.Handlers
{
    public class DirectorioEjecutivoHandler
    {
        private readonly DirectorioEjecutivoDAO _dao;

        public DirectorioEjecutivoHandler(DirectorioEjecutivoDAO dao)
        {
            _dao = dao;
        }

        public async Task<Respuesta> CrearAsync(UsuarioGeneral usuarioLogueado, List<DirectorioEjecutivoCrear> lstDirectorios)
        {
            try
            {
                return await _dao.CrearAsync(usuarioLogueado, lstDirectorios);
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<DirectorioEjecutivoCreado>() };
            }
        }

        public async Task<Respuesta> EditarAsync(UsuarioGeneral usuarioLogueado, DirectorioEjecutivoEditar request)
        {
            try
            {
                return await _dao.EditarAsync(usuarioLogueado, request);
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<DirectorioEjecutivoCreado>() };
            }
        }

        public async Task<Respuesta> ObtenerAsync(UsuarioGeneral usuarioLogueado, DirectorioEjecutivoObtenerRequest request)
        {
            try
            {
                return await _dao.ObtenerAsync(usuarioLogueado, request);
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<DirectorioEjecutivoConsulta>() };
            }
        }

        public async Task<Respuesta> ListarAsync(UsuarioGeneral usuarioLogueado, FiltroDirectorioEjecutivo filtro)
        {
            try
            {
                return await _dao.ListarAsync(usuarioLogueado, filtro);
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new DirectorioEjecutivoListaResult() };
            }
        }

        public async Task<Respuesta> EliminarAsync(UsuarioGeneral usuarioLogueado, int idDirectorioEjecutivo)
        {
            try
            {
                return await _dao.EliminarAsync(usuarioLogueado, idDirectorioEjecutivo);
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = new List<DirectorioEjecutivoEliminado>() };
            }
        }
    }
}
