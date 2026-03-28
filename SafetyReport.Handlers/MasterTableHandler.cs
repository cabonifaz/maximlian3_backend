using SafetyReport.DAO;
using SafetyReport.Models;

namespace SafetyReport.Handlers
{
    public class MasterTableHandler
    {
        private readonly MasterTableDAO _dao;

        public MasterTableHandler(MasterTableDAO dao)
        {
            _dao = dao;
        }

        public async Task<Respuesta> ListarAsync(UsuarioGeneral usuarioLogueado, int? idMaster)
        {
            try
            {
                return await _dao.ListarAsync(usuarioLogueado, idMaster);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<MasterTableItem>()
                };
            }
        }

        public async Task<Respuesta> ListarInventarioAsync(UsuarioGeneral usuarioLogueado)
        {
            try
            {
                return await _dao.ListarInventarioAsync(usuarioLogueado);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<InventarioMaestroItem>()
                };
            }
        }

        public async Task<Respuesta> CrearAsync(UsuarioGeneral usuarioLogueado, MasterTableRequest request)
        {
            try
            {
                return await _dao.CrearAsync(usuarioLogueado, request);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<MasterTableResultado>()
                };
            }
        }

        public async Task<Respuesta> EditarAsync(UsuarioGeneral usuarioLogueado, EditarMasterTableRequest request)
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
                    Mensaje = ex.Message,
                    Result = new List<MasterTableResultado>()
                };
            }
        }

        public async Task<Respuesta> EliminarAsync(UsuarioGeneral usuarioLogueado, EliminarMasterTableRequest request)
        {
            try
            {
                return await _dao.EliminarAsync(usuarioLogueado, request.IdMasterTable);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<MasterTableResultado>()
                };
            }
        }
    }
}