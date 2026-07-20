using SafetyReport.DAO;
using SafetyReport.Models;

namespace SafetyReport.Handlers
{
    public class FormatoDocumentoResolver
    {
        private readonly TablaMaestraDAO _tablaMaestraDao;

        public FormatoDocumentoResolver(TablaMaestraDAO tablaMaestraDao)
        {
            _tablaMaestraDao = tablaMaestraDao;
        }

        public async Task<string> ResolverAsync(UsuarioGeneral usuarioLogueado, string? formatoArchivo, string nombreArchivo)
        {
            var mime = (formatoArchivo ?? string.Empty).Trim().ToUpperInvariant();

            if (!string.IsNullOrWhiteSpace(mime))
            {
                var respuesta = await _tablaMaestraDao.ObtenerAsync(usuarioLogueado, new ObtenerTablaMaestraRequest
                {
                    idMaestro = 34
                });
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
