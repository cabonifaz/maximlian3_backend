namespace SafetyReport.Application.Puertos.Almacenamiento;

public interface ILogStorage
{
    Task UploadStreamAsync(string rutaArchivo, Stream stream, string contentType);
    Task<byte[]?> DescargarBytesAsync(string rutaArchivo);
}
