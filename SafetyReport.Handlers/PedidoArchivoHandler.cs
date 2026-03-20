using SafetyReport.DAO;
using SafetyReport.Models;

namespace SafetyReport.Handlers
{
    public class PedidoArchivoHandler
    {
        private readonly PedidoArchivoDAO _dao;
        private readonly IS3UploadService _s3UploadService;

        public PedidoArchivoHandler(PedidoArchivoDAO dao, IS3UploadService s3UploadService)
        {
            _dao = dao;
            _s3UploadService = s3UploadService;
        }

        public async Task<Respuesta> CrearAsync(UsuarioGeneral usuarioLogueado, PedidoArchivoCrear request)
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
                    Result = new List<PedidoArchivoCreado>()
                };
            }
        }

        public async Task<Respuesta> SubirAsync(UsuarioGeneral usuarioLogueado, PedidoArchivoSubirRequest request)
        {
            try
            {
                if (request.Archivo == null || request.Archivo.Length == 0)
                {
                    return new Respuesta
                    {
                        IdTipoMensaje = 1,
                        Mensaje = "Archivo inválido",
                        Result = new List<PedidoArchivoSubido>()
                    };
                }

                var rutaArchivo = _s3UploadService.GenerarRutaPedidoArchivo(request.IdPedido, request.NombreArchivo);

                await _s3UploadService.UploadFileAsync(rutaArchivo, request.Archivo);

                var crearRequest = new PedidoArchivoCrear
                {
                    IdPedido = request.IdPedido,
                    DocumentoURL = rutaArchivo,
                    NombreDocumento = request.NombreArchivo,
                    FormatoDocumento = request.TipoArchivo
                };

                var daoResponse = await _dao.CrearAsync(usuarioLogueado, crearRequest);

                if (daoResponse.IdTipoMensaje != 2)
                    return daoResponse;

                var idCreado = (daoResponse.Result as List<PedidoArchivoCreado>)?.FirstOrDefault()?.IdPedidoArchivo ?? 0;

                return new Respuesta
                {
                    IdTipoMensaje = 2,
                    Mensaje = daoResponse.Mensaje,
                    Result = new List<PedidoArchivoSubido>
                    {
                        new PedidoArchivoSubido { IdPedidoArchivo = idCreado, RutaArchivo = rutaArchivo }
                    }
                };
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 1,
                    Mensaje = ex.Message,
                    Result = new List<PedidoArchivoSubido>()
                };
            }
        }

        public async Task<Respuesta> EditarAsync(UsuarioGeneral usuarioLogueado, PedidoArchivoEditar request)
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
                    Result = new List<PedidoArchivoCreado>()
                };
            }
        }

        public async Task<Respuesta> ObtenerAsync(UsuarioGeneral usuarioLogueado, PedidoArchivoIdRequest request)
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
                    Result = new List<PedidoArchivoConsulta>()
                };
            }
        }

        public async Task<Respuesta> ListarAsync(UsuarioGeneral usuarioLogueado, FiltroPedidoArchivo request)
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
                    Result = new PedidoArchivoListaResult()
                };
            }
        }

        public async Task<Respuesta> EliminarAsync(UsuarioGeneral usuarioLogueado, PedidoArchivoIdRequest request)
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
                    Result = new List<PedidoArchivoEliminado>()
                };
            }
        }
    }
}