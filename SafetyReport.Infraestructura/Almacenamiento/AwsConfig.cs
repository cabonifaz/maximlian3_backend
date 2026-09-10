namespace SafetyReport.Infrastructure.Almacenamiento;

public class AwsConfig
{
    public string Region { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public int S3ExpirationTime { get; set; } = 15;
}
