using System.Text.Json.Serialization;

namespace SafetyReport.Models
{
    public class CognitoLoginRequest
    {
        [JsonPropertyName("custom:id_empresa")]
        public string IdEmpresa { get; set; }

        [JsonPropertyName("custom:id_usuario")]
        public string IdUsuario { get; set; }

        [JsonPropertyName("cognito:username")]
        public string Usuario { get; set; }
    }
}