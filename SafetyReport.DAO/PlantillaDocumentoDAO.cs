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
            using SqlCommand cmd = new("PlantillaDocumento_Obtener", cn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@intIdPlantillaDocumento", SqlDbType.Int).Value = idPlantilla;
            await cn.OpenAsync();

            using var dr = await cmd.ExecuteReaderAsync();
            if (!await dr.ReadAsync()) return null;

            return new PlantillaDocumento
            {
                Html       = dr["Html"]?.ToString() ?? string.Empty,
                Imagenes   = JsonSerializer.Deserialize<List<string>>(
                                 dr["Imagenes"]?.ToString() ?? "[]") ?? new()
            };
        }
    }
}
