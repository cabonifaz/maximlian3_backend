using SafetyReport.Models;

namespace SafetyReport.Application.Puertos.PedidoFactura;

public interface IPedidoPrefacturaExcelExporter
{
    byte[] GenerarExcelPrefactura(List<string> headers, List<PedidoPrefacturaConsulta> items);
}
