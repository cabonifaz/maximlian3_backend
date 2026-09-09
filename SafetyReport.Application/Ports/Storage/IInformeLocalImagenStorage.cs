namespace SafetyReport.Application.Ports.Storage;

public interface IInformeLocalImagenStorage
{
    List<string> GenerarDownloadUrlsBatch(List<string> rutasArchivo);
}
