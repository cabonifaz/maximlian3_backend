using SafetyReport.Models;

namespace SafetyReport.Application.Ports.Compania;

public interface ICompaniaNoticiasDetalleExcelExporter
{
    byte[] GenerarExcelNoticiasDetalle(List<CompaniaNoticiaDetalleListaConsulta> items);
}
