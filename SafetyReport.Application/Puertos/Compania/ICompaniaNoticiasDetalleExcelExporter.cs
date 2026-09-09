using SafetyReport.Models;

namespace SafetyReport.Application.Puertos.Compania;

public interface ICompaniaNoticiasDetalleExcelExporter
{
    byte[] GenerarExcelNoticiasDetalle(List<CompaniaNoticiaDetalleListaConsulta> items);
}
