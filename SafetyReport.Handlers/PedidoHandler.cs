using SafetyReport.DAO;
using SafetyReport.Models;

namespace SafetyReport.Handlers
{
    public class PedidoHandler
    {
        private readonly PedidoDAO _dao;
        private readonly PedidoArchivoDAO _pedidoArchivoDao;
        private readonly IS3UploadService _s3UploadService;
        private readonly TablaMaestraDAO _tablaMaestraDao;

        public PedidoHandler(PedidoDAO dao, PedidoArchivoDAO pedidoArchivoDao, IS3UploadService s3UploadService, TablaMaestraDAO tablaMaestraDao)
        {
            _dao = dao;
            _pedidoArchivoDao = pedidoArchivoDao;
            _s3UploadService = s3UploadService;
            _tablaMaestraDao = tablaMaestraDao;
        }

        public async Task<Respuesta> CrearAsync(UsuarioGeneral usuarioLogueado, Pedido request)
        {
            try
            {
                // Crea el pedido primero (sin archivos) para obtener el IdPedido real.
                var respuestaDao = await _dao.CrearAsync(usuarioLogueado, request);

                if (respuestaDao.IdTipoMensaje != 2)
                    return respuestaDao;

                var pedidos = respuestaDao.Result as List<PedidoCreado> ?? new List<PedidoCreado>();
                var idPedido = pedidos.FirstOrDefault()?.IdPedido ?? 0;

                var respuesta = new PedidoCreadoResponse { IdPedido = idPedido };

                if (idPedido > 0 && request.Archivos != null && request.Archivos.Count > 0)
                {
                    foreach (var archivo in request.Archivos)
                    {
                        var formatoDocumento = await ResolverFormatoDocumentoAsync(usuarioLogueado, archivo.FormatoArchivo, archivo.NombreDocumento);
                        var rutaDefecto = _s3UploadService.GenerarRutaPedidoArchivo(idPedido, archivo.NombreDocumento, 0);

                        var archivoCrear = new PedidoArchivoCrear
                        {
                            IdPedido = idPedido,
                            DocumentoURL = rutaDefecto,
                            NombreDocumento = archivo.NombreDocumento,
                            FormatoDocumento = formatoDocumento,
                            TamanoArchivo = archivo.TamanoArchivo,
                            IdTipoArchivo = archivo.IdTipoArchivo
                        };

                        var respuestaArchivo = await _pedidoArchivoDao.CrearAsync(usuarioLogueado, archivoCrear);

                        if (respuestaArchivo.IdTipoMensaje != 2)
                        {
                            return new Respuesta
                            {
                                IdTipoMensaje = respuestaArchivo.IdTipoMensaje,
                                Mensaje = respuestaArchivo.Mensaje,
                                Result = new List<PedidoCreadoResponse>()
                            };
                        }

                        var archivosCreados = respuestaArchivo.Result as List<PedidoArchivoCreado> ?? [];
                        var rutaArchivo = archivosCreados.FirstOrDefault()?.DocumentoURL ?? rutaDefecto;

                        respuesta.Archivos.Add(new PedidoArchivoPresignado
                        {
                            NombreDocumento = archivo.NombreDocumento,
                            RutaArchivo = rutaArchivo,
                            UploadUrl = _s3UploadService.GenerarUploadUrl(rutaArchivo, archivo.FormatoArchivo)
                        });
                    }
                }

                return new Respuesta
                {
                    IdTipoMensaje = 2,
                    Mensaje = respuestaDao.Mensaje,
                    Result = new List<PedidoCreadoResponse> { respuesta }
                };
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = "Error interno del servidor.",
                    Result = new List<PedidoCreadoConArchivos>()
                };
            }
        }

        public async Task<Respuesta> EditarAsync(UsuarioGeneral usuarioLogueado, EditarPedido request)
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
                    Result = new List<PedidoCreado>()
                };
            }
        }

        public async Task<Respuesta> ObtenerAsync(UsuarioGeneral usuarioLogueado, FiltroPedidoObtener request)
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
                    Result = new List<PedidoConsulta>()
                };
            }
        }

        public async Task<Respuesta> ListarAsync(UsuarioGeneral usuarioLogueado, FiltroPedido request)
        {
            try
            {
                return await _dao.ListarAsync(usuarioLogueado, request);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = "Error interno del servidor.",
                    Result = new PedidoListaResult()
                };
            }
        }

        public async Task<Respuesta> ListarAsignacionAsync(UsuarioGeneral usuarioLogueado, FiltroPedidoAsignacion request)
        {
            try
            {
                return await _dao.ListarAsignacionAsync(usuarioLogueado, request);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = "Error interno del servidor.",
                    Result = new PedidoAsignacionListaResult()
                };
            }
        }

        public async Task<Respuesta> CancelarAsync(UsuarioGeneral usuarioLogueado, PedidoIdRequest request)
        {
            try
            {
                return await _dao.CancelarAsync(usuarioLogueado, request.IdPedido);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = "Error interno del servidor.",
                    Result = new List<PedidoEliminado>()
                };
            }
        }

        public async Task<Respuesta> EliminarAsync(UsuarioGeneral usuarioLogueado, PedidoIdRequest request)
        {
            try
            {
                return await _dao.EliminarAsync(usuarioLogueado, request.IdPedido);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = "Error interno del servidor.",
                    Result = new List<PedidoEliminado>()
                };
            }
        }

        private async Task<string> ResolverFormatoDocumentoAsync(UsuarioGeneral usuarioLogueado, string formatoArchivo, string nombreArchivo)
        {
            var mime = (formatoArchivo ?? string.Empty).Trim().ToUpperInvariant();

            if (!string.IsNullOrWhiteSpace(mime))
            {
                var respuesta = await _tablaMaestraDao.ListarAsync(usuarioLogueado, "34");
                if (respuesta.IdTipoMensaje == 2 && respuesta.Result is List<TablaMaestraItem> items)
                {
                    var match = items.FirstOrDefault(i =>
                        string.Equals(i.String3, mime, StringComparison.OrdinalIgnoreCase));
                    if (match?.String1 != null)
                        return match.String1.ToUpperInvariant();
                }
            }

            return Path.GetExtension(nombreArchivo).TrimStart('.').ToUpperInvariant();
        }
    }
}