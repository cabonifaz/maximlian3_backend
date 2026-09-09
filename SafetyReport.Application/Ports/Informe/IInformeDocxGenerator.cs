using System.Text.Json.Nodes;

namespace SafetyReport.Application.Ports.Informe;

public interface IInformeDocxGenerator
{
    MemoryStream GenerarDocx(JsonNode json, Dictionary<string, byte[]>? assets = null);
}
