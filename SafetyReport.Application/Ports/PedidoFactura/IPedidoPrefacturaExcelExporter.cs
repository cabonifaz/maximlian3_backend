using SafetyReport.Models;

namespace SafetyReport.Application.Ports.PedidoFactura;

public interface IPedidoPrefacturaExcelExporter
{
    byte[] GenerarExcelPrefactura(List<string> headers, List<PedidoPrefacturaConsulta> items);
}
