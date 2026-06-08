using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SafetyReport.Models;

public class S3UploadService : IS3UploadService
{
    private readonly IAmazonS3 _s3Client;
    private readonly IConfiguration _configuration;
    private readonly string _bucketName;
    private readonly int _s3ExpirationMinutes;

    public S3UploadService(IAmazonS3 s3Client, IConfiguration configuration)
    {
        _s3Client = s3Client;
        _configuration = configuration;
        _bucketName = _configuration["AWS:BucketName"] ?? throw new Exception("Falta AWS:BucketName");
        _s3ExpirationMinutes = int.TryParse(_configuration["AWS:S3ExpirationTime"], out var exp) ? exp : 15;
    }

    public string GenerarRutaPedidoArchivo(int idPedido, string nombreArchivo, int idArchivo)
    {
        var extension = Path.GetExtension(nombreArchivo).TrimStart('.');
        var nombreBase = Path.GetFileNameWithoutExtension(nombreArchivo);
        var idArchivoCreado = idArchivo == 0 ? "[IdArchivo]" : idArchivo.ToString();

        var nombreLimpio = string.Concat(
            nombreBase.Where(c => char.IsLetterOrDigit(c) || c == '_' || c == '-')
        );

        if (string.IsNullOrWhiteSpace(nombreLimpio))
            nombreLimpio = "archivo";

        return $"pedidos/{idPedido}/{idArchivoCreado}/{nombreLimpio}.{extension}";
    }

    public string GenerarDownloadUrl(string rutaArchivo)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = rutaArchivo,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.AddMinutes(_s3ExpirationMinutes)
        };

        return _s3Client.GetPreSignedURL(request);
    }

    public string GenerarUploadUrl(string rutaArchivo, string formatoArchivo)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = rutaArchivo,
            Verb = HttpVerb.PUT,
            Expires = DateTime.UtcNow.AddMinutes(_s3ExpirationMinutes)
        };

        return _s3Client.GetPreSignedURL(request);
    }

    public List<string> GenerarUploadUrlsBatch(List<string> sufijos, string formatoArchivo)
    {
        var expiry = DateTime.UtcNow.AddMinutes(_s3ExpirationMinutes);
        return sufijos.Select(sufijo => _s3Client.GetPreSignedURL(new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key        = sufijo,
            Verb       = HttpVerb.PUT,
            Expires    = expiry
        })).ToList();
    }

    public List<string> GenerarDownloadUrlsBatch(List<string> sufijos)
    {
        var expiry = DateTime.UtcNow.AddMinutes(_s3ExpirationMinutes);
        return sufijos.Select(sufijo => _s3Client.GetPreSignedURL(new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key        = sufijo,
            Verb       = HttpVerb.GET,
            Expires    = expiry
        })).ToList();
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

    public async Task DeleteFileAsync(string rutaArchivo)
    {
        if (string.IsNullOrWhiteSpace(rutaArchivo))
        {
            throw new ArgumentException("rutaArchivo es requerido", nameof(rutaArchivo));
        }

        var deleteRequest = new DeleteObjectRequest
        {
            BucketName = _bucketName,
            Key = rutaArchivo
        };

        await _s3Client.DeleteObjectAsync(deleteRequest);
    }

    public async Task MoverArchivoAsync(string rutaOrigen, string rutaDestino)
    {
        if (string.IsNullOrWhiteSpace(rutaOrigen))
            throw new ArgumentException("rutaOrigen es requerido", nameof(rutaOrigen));
        if (string.IsNullOrWhiteSpace(rutaDestino))
            throw new ArgumentException("rutaDestino es requerido", nameof(rutaDestino));

        var copyRequest = new CopyObjectRequest
        {
            SourceBucket = _bucketName,
            SourceKey = rutaOrigen,
            DestinationBucket = _bucketName,
            DestinationKey = rutaDestino,
            MetadataDirective = S3MetadataDirective.COPY
        };

        await _s3Client.CopyObjectAsync(copyRequest);

        await DeleteFileAsync(rutaOrigen);
    }

}