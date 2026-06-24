using Microsoft.Extensions.Logging;
using SafetyReport.DAO;
using SafetyReport.Models;

namespace SafetyReport.Handlers
{
    public class TablaMaestraHandler
    {
        private readonly TablaMaestraDAO _dao;
        private readonly BedrockTranslationService _translator;
        private readonly ILogger<TablaMaestraHandler> _logger;

        public TablaMaestraHandler(TablaMaestraDAO dao, BedrockTranslationService translator, ILogger<TablaMaestraHandler> logger)
        {
            _dao = dao;
            _translator = translator;
            _logger = logger;
        }

        public async Task<Respuesta> ListarAsync(UsuarioGeneral usuarioLogueado, int? idMaestro)
        {
            try
            {
                return await _dao.ListarAsync(usuarioLogueado, idMaestro);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = "Error interno del servidor.",
                    Result = new List<TablaMaestraItem>()
                };
            }
        }

        public async Task<Respuesta> ListarInventarioAsync(UsuarioGeneral usuarioLogueado, int? idMaestro)
        {
            try
            {
                return await _dao.ListarInventarioAsync(usuarioLogueado, idMaestro);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = "Error interno del servidor.",
                    Result = new List<InventarioMaestroItem>()
                };
            }
        }

        private static readonly HashSet<int> _maestrosSoloString1 = new() { 44, 45 };

        public async Task<Respuesta> CrearAsync(UsuarioGeneral usuarioLogueado, TablaMaestraRequest request)
        {
            try
            {
                var respuesta = await _dao.CrearAsync(usuarioLogueado, request);

                if (respuesta.IdTipoMensaje == 2)
                {
                    var input = _maestrosSoloString1.Contains(request.IdMaestro)
                        ? new TranslationInput { String1 = request.String1 }
                        : new TranslationInput { String1 = request.String1, String2 = request.String2 };

                    var usuario = usuarioLogueado;
                    var req = request;

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            _logger.LogInformation("[Traduccion] Iniciando traduccion para IdMaestro={IdMaestro}, Num1={Num1}, String1={String1}, String2={String2}",
                                req.IdMaestro, req.Num1, input.String1, input.String2);

                            var traduccion = await _translator.TranslateAsync(input);

                            _logger.LogInformation("[Traduccion] Respuesta AI: String4={String4}, String5={String5}, String6={String6}, String7={String7}",
                                traduccion.String4, traduccion.String5, traduccion.String6, traduccion.String7);

                            var resultado = await _dao.ActualizarTraduccionesAsync(usuario, req.IdMaestro, req.Num1, req.Num2, req.Num3, traduccion.String4, traduccion.String5, traduccion.String6, traduccion.String7);

                            _logger.LogInformation("[Traduccion] SP resultado: IdTipoMensaje={IdTipoMensaje}, Mensaje={Mensaje}",
                                resultado.IdTipoMensaje, resultado.Mensaje);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "[Traduccion] Error al traducir IdMaestro={IdMaestro}, Num1={Num1}", req.IdMaestro, req.Num1);
                        }
                    });
                }

                return respuesta;
            }
            catch (Exception)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = "Error interno del servidor.",
                    Result = new List<TablaMaestraResultado>()
                };
            }
        }

        public async Task<Respuesta> EditarAsync(UsuarioGeneral usuarioLogueado, EditarTablaMaestraRequest request)
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
                    Result = new List<TablaMaestraResultado>()
                };
            }
        }

        public async Task<Respuesta> ObtenerAsync(UsuarioGeneral usuarioLogueado, ObtenerTablaMaestraRequest request)
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
                    Result = new List<TablaMaestraItem>()
                };
            }
        }

        public async Task<Respuesta> EliminarAsync(UsuarioGeneral usuarioLogueado, EliminarTablaMaestraRequest request)
        {
            try
            {
                return await _dao.EliminarAsync(usuarioLogueado, request.IdTablaMaestra);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = "Error interno del servidor.",
                    Result = new List<TablaMaestraResultado>()
                };
            }
        }
    }
}