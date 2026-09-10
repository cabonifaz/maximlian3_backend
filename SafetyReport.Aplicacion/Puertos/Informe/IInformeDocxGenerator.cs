using System.Text.Json.Nodes;

namespace SafetyReport.Application.Puertos.Informe;

public interface IInformeDocxGenerator
{
    MemoryStream GenerarDocx(JsonNode json, Dictionary<string, byte[]>? assets = null);
}
