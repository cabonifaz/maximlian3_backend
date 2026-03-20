using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

public class S3UploadService : IS3UploadService
{
    private readonly IAmazonS3 _s3Client;
    private readonly IConfiguration _configuration;
    private readonly string _bucketName;

    public S3UploadService(IAmazonS3 s3Client, IConfiguration configuration)
    {
        _s3Client = s3Client;
        _configuration = configuration;
        _bucketName = _configuration["AWS:BucketName"] ?? throw new Exception("Falta AWS:BucketName");
    }

    public string GenerarRutaPedidoArchivo(int idPedido, string nombreArchivo)
    {
        var extension = Path.GetExtension(nombreArchivo);
        var nombreBase = Path.GetFileNameWithoutExtension(nombreArchivo);

        var nombreLimpio = string.Concat(
            nombreBase.Where(c => char.IsLetterOrDigit(c) || c == '_' || c == '-')
        );

        if (string.IsNullOrWhiteSpace(nombreLimpio))
            nombreLimpio = "archivo";

        return $"pedidos/{idPedido}/{DateTime.UtcNow:yyyyMMddHHmmssfff}{Guid.NewGuid():N}{nombreLimpio}{extension}";
    }

    public string GenerarUploadUrl(string rutaArchivo, string tipoArchivo)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = rutaArchivo,
            Verb = HttpVerb.PUT,
            Expires = DateTime.UtcNow.AddMinutes(15),
            ContentType = tipoArchivo
        };

        return _s3Client.GetPreSignedURL(request);
    }

    public async Task UploadFileAsync(string rutaArchivo, IFormFile file)
    {
        if (file is null || file.Length == 0)
        {
            throw new ArgumentException("Archivo no válido", nameof(file));
        }

        using var stream = file.OpenReadStream();

        var putRequest = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = rutaArchivo,
            InputStream = stream,
            ContentType = file.ContentType,
            AutoCloseStream = true
        };

        await _s3Client.PutObjectAsync(putRequest);
    }
}