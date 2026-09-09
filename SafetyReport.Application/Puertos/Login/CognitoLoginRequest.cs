using System.Text.Json.Serialization;

namespace SafetyReport.Application.Puertos.Login
{
    public class CognitoLoginRequest
    {
        [JsonPropertyName("custom:id_empresa")]
        public string IdEmpresa { get; set; } = string.Empty;

        [JsonPropertyName("custom:id_usuario")]
        public string IdUsuario { get; set; } = string.Empty;

        [JsonPropertyName("cognito:username")]
        public string Usuario { get; set; } = string.Empty;
    }
}
