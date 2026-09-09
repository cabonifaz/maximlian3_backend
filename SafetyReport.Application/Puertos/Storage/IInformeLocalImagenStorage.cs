namespace SafetyReport.Application.Puertos.Storage;

public interface IInformeLocalImagenStorage
{
    List<string> GenerarDownloadUrlsBatch(List<string> rutasArchivo);
}
