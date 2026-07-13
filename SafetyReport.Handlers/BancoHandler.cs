using SafetyReport.DAO;
using SafetyReport.Models;

namespace SafetyReport.Handlers
{
    public class BancoHandler
    {
        private readonly BancoDAO _dao;

        public BancoHandler(BancoDAO dao)
        {
            _dao = dao;
        }

        public async Task<Respuesta> CrearAsync(UsuarioGeneral usuarioLogueado, List<BancoCrear> lstBancos)
        {
            try
            {
                return await _dao.CrearAsync(usuarioLogueado, lstBancos);
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new List<BancoCreado>() };
            }
        }

        public async Task<Respuesta> EditarAsync(UsuarioGeneral usuarioLogueado, BancoEditar request)
        {
            try
            {
                return await _dao.EditarAsync(usuarioLogueado, request);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = "Error interno del servidor.",
                    Result = new List<BancoCreado>()
                };
            }
        }

        public async Task<Respuesta> ObtenerAsync(UsuarioGeneral usuarioLogueado, BancoObtenerRequest request)
        {
            try
            {
                return await _dao.ObtenerAsync(usuarioLogueado, request);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = "Error interno del servidor.",
                    Result = new List<BancoConsulta>()
                };
            }
        }

        public async Task<Respuesta> ListarAsync(UsuarioGeneral usuarioLogueado, FiltroBanco filtro)
        {
            try
            {
                return await _dao.ListarAsync(usuarioLogueado, filtro);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = "Error interno del servidor.",
                    Result = new BancoListaResult()
                };
            }
        }

        public async Task<Respuesta> ListarMatchAsync(UsuarioGeneral usuarioLogueado, List<BancoMatchItem> lista)
        {
            try
            {
                return await _dao.ListarMatchAsync(usuarioLogueado, lista);
            }
            catch (Exception ex)
            {
                return new Respuesta { IdTipoMensaje = 3, Mensaje = "Error interno del servidor.", Result = new List<BancoMatchResultItem>() };
            }
        }

        public async Task<Respuesta> EliminarAsync(UsuarioGeneral usuarioLogueado, int idBanco)
        {
            try
            {
                return await _dao.EliminarAsync(usuarioLogueado, idBanco);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = "Error interno del servidor.",
                    Result = new List<BancoEliminado>()
                };
            }
        }
    }
}
