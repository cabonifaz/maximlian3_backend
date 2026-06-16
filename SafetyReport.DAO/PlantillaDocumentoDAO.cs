using Microsoft.Data.SqlClient;
using SafetyReport.Models;
using System.Data;
using System.Text.Json;

namespace SafetyReport.DAO
{
    public class PlantillaDocumentoDAO
    {
        private readonly DbConfig _dbConfig;

        public PlantillaDocumentoDAO(DbConfig dbConfig)
        {
            _dbConfig = dbConfig;
        }

        public async Task<PlantillaDocumento?> ObtenerPorIdAsync(int idPlantilla)
        {
            using SqlConnection cn = new(_dbConfig.ConnectionString);
            using SqlCommand cmd = new(
                "SELECT IdPlantillaDocumento, Nombre, Descripcion, Formato, Estructura, Imagenes " +
                "FROM PLANTILLA_DOCUMENTO " +
                "WHERE IdPlantillaDocumento = @id AND SoftDelete = 0", cn);

            cmd.Parameters.Add("@id", SqlDbType.Int).Value = idPlantilla;
            await cn.OpenAsync();

            using var dr = await cmd.ExecuteReaderAsync();
            if (!await dr.ReadAsync()) return null;

            return new PlantillaDocumento
            {
                IdPlantillaDocumento = Convert.ToInt32(dr["IdPlantillaDocumento"]),
                Nombre               = dr["Nombre"]?.ToString() ?? string.Empty,
                Descripcion          = dr["Descripcion"]?.ToString(),
                Formato              = dr["Formato"]?.ToString() ?? string.Empty,
                Estructura           = dr["Estructura"]?.ToString() ?? string.Empty,
                Imagenes             = JsonSerializer.Deserialize<List<string>>(
                                           dr["Imagenes"]?.ToString() ?? "[]") ?? new()
            };
        }
    }
}
