using System.Text.Json.Nodes;

namespace SafetyReport.Application.Ports.Informe;

public interface IInformePdfGenerator
{
    MemoryStream GenerarPdf(JsonNode json, Dictionary<string, byte[]>? assets = null);
    HashSet<string> DetectarVariantesFuenteDocumento(JsonNode json);
    Dictionary<string, string> ObtenerRutasS3FuentesDocumento(string fontFamily, HashSet<string> variantes);
    void ConfigurarFuentesDocumento(Dictionary<string, byte[]> fuentes);
}
