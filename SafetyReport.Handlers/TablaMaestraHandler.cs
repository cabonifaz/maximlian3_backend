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

        public async Task<Respuesta> ListarAsync(UsuarioGeneral usuarioLogueado, string? idsMaestro)
        {
            try
            {
                return await _dao.ListarAsync(usuarioLogueado, idsMaestro);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = "Error interno del servidor.",
                    Result = new List<TablaMaestraGroup>()
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
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = "Error interno del servidor.",
                    Result = new List<InventarioMaestroItem>()
                };
            }
        }

        private static readonly HashSet<int> _maestrosSoloString1 = new() { 14, 44, 45, 47, 48, 49, 52, 56, 57, 58, 59, 60, 61 };

        public async Task<Respuesta> CrearAsync(UsuarioGeneral usuarioLogueado, TablaMaestraRequest request)
        {
            try
            {
                var soloString1 = _maestrosSoloString1.Contains(request.IdMaestro);
                var input = new TranslationInput
                {
                    String1 = request.InputText,
                    String2 = soloString1 ? null : request.InputText2,
                    IdIdioma = request.IdIdioma ?? 1
                };

                try
                {
                    var traduccion = await _translator.TranslateAsync(input);

                    switch (request.IdIdioma)
                    {
                        case 2: // input is English
                            request.String4 = request.InputText;
                            request.String5 = request.InputText2;
                            request.String1 = traduccion.String1;
                            request.String2 = traduccion.String2;
                            request.String6 = traduccion.String6;
                            request.String7 = traduccion.String7;
                            break;
                        case 3: // input is Portuguese
                            request.String6 = request.InputText;
                            request.String7 = request.InputText2;
                            request.String1 = traduccion.String1;
                            request.String2 = traduccion.String2;
                            request.String4 = traduccion.String4;
                            request.String5 = traduccion.String5;
                            break;
                        default: // input is Spanish
                            request.String1 = request.InputText;
                            request.String2 = request.InputText2;
                            request.String4 = traduccion.String4;
                            request.String5 = traduccion.String5;
                            request.String6 = traduccion.String6;
                            request.String7 = traduccion.String7;
                            break;
                    }
                }
                catch
                {
                    return new Respuesta
                    {
                        IdTipoMensaje = 1,
                        Mensaje = "La traducción al inglés y portugués falló, intente nuevamente.",
                        Result = new List<TablaMaestraResultado>()
                    };
                }

                return await _dao.CrearAsync(usuarioLogueado, request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

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
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

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
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

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
                _logger.LogError(ex, "Error no controlado en la capa de negocio.");

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