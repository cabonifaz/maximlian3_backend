using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SafetyReport.Models;

public class S3UploadService : IS3UploadService
{
    private readonly IAmazonS3 _s3Client;
    private readonly IAmazonSecurityTokenService _stsClient;
    private readonly IConfiguration _configuration;
    private readonly string _bucketName;

    public S3UploadService(IAmazonS3 s3Client, IAmazonSecurityTokenService stsClient, IConfiguration configuration)
    {
        _s3Client = s3Client;
        _stsClient = stsClient;
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

    public async Task<CredencialesTemporalesS3> ObtenerCredencialesTemporalesAsync(int idPedido)
    {
        var policy = $$"""
            {
              "Version": "2012-10-17",
              "Statement": [{
                "Effect": "Allow",
                "Action": ["s3:PutObject"],
                "Resource": "arn:aws:s3:::{{_bucketName}}/pedidos/{{idPedido}}/*"
              }]
            }
            """;

        var request = new GetFederationTokenRequest
        {
            Name = $"pedido-{idPedido}",
            Policy = policy,
            DurationSeconds = 900
        };

        var response = await _stsClient.GetFederationTokenAsync(request);

        return new CredencialesTemporalesS3
        {
            AccessKeyId = response.Credentials.AccessKeyId,
            SecretAccessKey = response.Credentials.SecretAccessKey,
            SessionToken = response.Credentials.SessionToken,
            Expiration = response.Credentials.Expiration ?? DateTime.UtcNow.AddMinutes(15)
        };
    }
}