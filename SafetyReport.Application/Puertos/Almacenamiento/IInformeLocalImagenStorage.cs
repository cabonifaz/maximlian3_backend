namespace SafetyReport.Application.Puertos.Almacenamiento;

public interface IInformeLocalImagenStorage
{
    List<string> GenerarDownloadUrlsBatch(List<string> rutasArchivo);
}
