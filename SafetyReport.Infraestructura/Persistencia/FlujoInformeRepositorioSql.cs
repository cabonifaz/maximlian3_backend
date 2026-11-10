using MySqlConnector;
using Microsoft.Extensions.Logging;
using SafetyReport.Application.Puertos.FlujoInforme;
using System.Data;

namespace SafetyReport.Infrastructure.Persistencia
{
    // SRC-F1-065: SP_Asignacion_ListarPorEtapa, SP_Asignacion_AsignarRol, SP_InformeRepositorio_Listar,
    // SP_InformeComplemento_Crear / _Listar / _EnviarAprobacion (ver asignaciones.sql).
    // Todos devuelven: result set 1 = IdTipoMensaje/Mensaje; en éxito, los siguientes con los datos.
    public class FlujoInformeRepositorioSql : IFlujoInformeRepository
    {
        private readonly DbConfig _dbConfig;
        private readonly ILogger<FlujoInformeRepositorioSql> _logger;

        public FlujoInformeRepositorioSql(DbConfig dbConfig, ILogger<FlujoInformeRepositorioSql> logger)
        {
            _dbConfig = dbConfig;
            _logger = logger;
        }

        // ── Asignaciones por etapa ─────────────────────────────────────────────

        public Task<Respuesta> ListarPorEtapaAsync(UsuarioGeneral u, FiltroFlujoAsignacion filtro) =>
            EjecutarAsync("SP_Asignacion_ListarPorEtapa", u,
                cmd =>
                {
                    cmd.Parameters.Add("@p_vchEtapa", MySqlDbType.VarChar, 20).Value = filtro.Etapa;
                    cmd.Parameters.Add("@p_vchBusqueda", MySqlDbType.VarChar, 255).Value = Nulo(filtro.Busqueda);
                    cmd.Parameters.Add("@p_intNumPag", MySqlDbType.Int32).Value = (object?)filtro.NumPag ?? DBNull.Value;
                },
                async dr =>
                {
                    var resultado = new ListaFlujoAsignacion();
                    if (!await dr.NextResultAsync()) return resultado;

                    if (await dr.ReadAsync())
                    {
                        resultado.TotalRegistros = Entero(dr, "TotalRegistros");
                        resultado.TotalPaginas = Entero(dr, "TotalPaginas");
                        resultado.Conteos = new ConteoTabsFlujoAsignacion
                        {
                            SinAsignar = Entero(dr, "SinAsignar"),
                            EnAnalisis = Entero(dr, "EnAnalisis"),
                            ListoTraduccion = Entero(dr, "ListoTraduccion"),
                            EnTraduccion = Entero(dr, "EnTraduccion"),
                            PorRevisar = Entero(dr, "PorRevisar"),
                            Complementos = Entero(dr, "Complementos"),
                            Finalizados = Entero(dr, "Finalizados")
                        };
                    }

                    if (!await dr.NextResultAsync()) return resultado;

                    while (await dr.ReadAsync())
                    {
                        resultado.lstPedido.Add(new FilaFlujoAsignacion
                        {
                            IdPedido = Entero(dr, "IdPedido"),
                            Cliente = Texto(dr, "Cliente"),
                            Investigado = Texto(dr, "Investigado"),
                            Pais = Texto(dr, "Pais"),
                            IdIdioma = Entero(dr, "IdIdioma"),
                            IdiomaInforme = Texto(dr, "IdiomaInforme"),
                            RequiereTraduccion = Booleano(dr, "RequiereTraduccion"),
                            Prioridad = Texto(dr, "Prioridad") ?? "normal",
                            FechaPedido = FechaUtc(dr, "FechaPedido"),
                            FechaVencimiento = FechaUtc(dr, "FechaVencimiento"),
                            Etapa = Texto(dr, "Etapa") ?? string.Empty,
                            IdUsuarioAnalista = EnteroNulo(dr, "IdUsuarioAnalista"),
                            Analista = Texto(dr, "Analista"),
                            IdUsuarioTraductor = EnteroNulo(dr, "IdUsuarioTraductor"),
                            Traductor = Texto(dr, "Traductor"),
                            IdInformeAnalista = EnteroNulo(dr, "IdInformeAnalista"),
                            IdInformeTraductor = EnteroNulo(dr, "IdInformeTraductor"),
                            FechaFinAnalisis = FechaUtc(dr, "FechaFinAnalisis"),
                            Rechazado = Booleano(dr, "Rechazado"),
                            FechaFinalizacion = FechaUtc(dr, "FechaFinalizacion")
                        });
                    }

                    return resultado;
                },
                new ListaFlujoAsignacion());

        public Task<Respuesta> AsignarRolAsync(UsuarioGeneral u, int idPedido, int idRolAsignado, int idUsuarioAsignado) =>
            EjecutarAsync("SP_Asignacion_AsignarRol", u,
                cmd =>
                {
                    cmd.Parameters.Add("@p_intIdPedido", MySqlDbType.Int32).Value = idPedido;
                    cmd.Parameters.Add("@p_intIdRolAsignado", MySqlDbType.Int32).Value = idRolAsignado;
                    cmd.Parameters.Add("@p_intIdUsuarioAsignado", MySqlDbType.Int32).Value = idUsuarioAsignado;
                },
                async dr =>
                {
                    var resultado = new AsignacionFlujoCreada();
                    if (await dr.NextResultAsync() && await dr.ReadAsync())
                        resultado.IdAsignacion = Entero(dr, "IdAsignacion");
                    return resultado;
                },
                new AsignacionFlujoCreada());

        // ── Repositorio de informes ────────────────────────────────────────────

        public Task<Respuesta> ListarRepositorioAsync(UsuarioGeneral u, FiltroRepositorioInformes filtro) =>
            EjecutarAsync("SP_InformeRepositorio_Listar", u,
                cmd =>
                {
                    cmd.Parameters.Add("@p_vchBusqueda", MySqlDbType.VarChar, 255).Value = Nulo(filtro.Busqueda);
                    cmd.Parameters.Add("@p_vchTipo", MySqlDbType.VarChar, 20).Value = Nulo(filtro.Tipo);
                    cmd.Parameters.Add("@p_intNumPag", MySqlDbType.Int32).Value = (object?)filtro.NumPag ?? DBNull.Value;
                },
                async dr =>
                {
                    var resultado = new ListaRepositorioInformes();
                    if (!await dr.NextResultAsync()) return resultado;

                    if (await dr.ReadAsync())
                    {
                        resultado.TotalRegistros = Entero(dr, "TotalRegistros");
                        resultado.TotalPaginas = Entero(dr, "TotalPaginas");
                    }

                    if (!await dr.NextResultAsync()) return resultado;

                    while (await dr.ReadAsync())
                    {
                        resultado.lstInforme.Add(new InformeRepositorio
                        {
                            IdInforme = Entero(dr, "IdInforme"),
                            IdCompania = Entero(dr, "IdCompania"),
                            Tipo = Texto(dr, "Tipo") ?? "informe",
                            Investigado = Texto(dr, "Investigado"),
                            Pais = Texto(dr, "Pais"),
                            Cliente = Texto(dr, "Cliente"),
                            Idioma = Texto(dr, "Idioma"),
                            Traductor = Texto(dr, "Traductor"),
                            FechaAprobacion = FechaUtc(dr, "FechaAprobacion"),
                            IdInformeBase = EnteroNulo(dr, "IdInformeBase"),
                            EsUltimaVersion = Booleano(dr, "EsUltimaVersion")
                        });
                    }

                    return resultado;
                },
                new ListaRepositorioInformes());

        // ── Complementos ───────────────────────────────────────────────────────

        public Task<Respuesta> ListarComplementosAsync(UsuarioGeneral u, FiltroComplementos filtro) =>
            EjecutarAsync("SP_InformeComplemento_Listar", u,
                cmd =>
                {
                    cmd.Parameters.Add("@p_vchBusqueda", MySqlDbType.VarChar, 255).Value = Nulo(filtro.Busqueda);
                    cmd.Parameters.Add("@p_bitSoloPropios", MySqlDbType.Byte).Value = filtro.SoloPropios ? 1 : 0;
                    cmd.Parameters.Add("@p_intNumPag", MySqlDbType.Int32).Value = (object?)filtro.NumPag ?? DBNull.Value;
                },
                async dr =>
                {
                    var resultado = new ListaComplementos();
                    if (!await dr.NextResultAsync()) return resultado;

                    if (await dr.ReadAsync())
                    {
                        resultado.TotalRegistros = Entero(dr, "TotalRegistros");
                        resultado.TotalPaginas = Entero(dr, "TotalPaginas");
                    }

                    if (!await dr.NextResultAsync()) return resultado;

                    while (await dr.ReadAsync())
                    {
                        resultado.lstComplemento.Add(new Complemento
                        {
                            IdComplemento = Entero(dr, "IdComplemento"),
                            IdCompania = Entero(dr, "IdCompania"),
                            Investigado = Texto(dr, "Investigado"),
                            Pais = Texto(dr, "Pais"),
                            IdInformeBase = EnteroNulo(dr, "IdInformeBase"),
                            TipoInformeBase = Texto(dr, "TipoInformeBase") ?? "informe",
                            ClienteBase = Texto(dr, "ClienteBase"),
                            IdClienteDestino = Entero(dr, "IdClienteDestino"),
                            ClienteDestino = Texto(dr, "ClienteDestino"),
                            Idioma = Texto(dr, "Idioma"),
                            IdUsuarioTraductor = EnteroNulo(dr, "IdUsuarioTraductor"),
                            Traductor = Texto(dr, "Traductor"),
                            Estado = Texto(dr, "Estado") ?? "borrador",
                            FechaCreacion = FechaUtc(dr, "FechaCreacion"),
                            FechaModificacion = FechaUtc(dr, "FechaModificacion"),
                            IdInforme = EnteroNulo(dr, "IdInforme"),
                            Codigo = Texto(dr, "Codigo"),
                            IdPlantilla = EnteroNulo(dr, "IdPlantilla")
                        });
                    }

                    return resultado;
                },
                new ListaComplementos());

        public Task<Respuesta> CrearComplementoAsync(UsuarioGeneral u, CrearComplementoRequest request) =>
            EjecutarAsync("SP_InformeComplemento_Crear", u,
                cmd =>
                {
                    cmd.Parameters.Add("@p_intIdCompania", MySqlDbType.Int32).Value = request.IdCompania;
                    cmd.Parameters.Add("@p_intIdClienteDestino", MySqlDbType.Int32).Value = request.IdClienteDestino;
                    cmd.Parameters.Add("@p_intIdIdioma", MySqlDbType.Int32).Value = request.IdIdioma;
                    cmd.Parameters.Add("@p_vchObservacion", MySqlDbType.VarChar, 500).Value = Nulo(request.Observacion);
                },
                async dr =>
                {
                    var resultado = new ComplementoCreado();
                    if (await dr.NextResultAsync() && await dr.ReadAsync())
                    {
                        resultado.IdComplemento = Entero(dr, "IdComplemento");
                        resultado.Codigo = Texto(dr, "Codigo");
                        resultado.IdInformeBase = EnteroNulo(dr, "IdInformeBase");
                    }
                    return resultado;
                },
                new ComplementoCreado());

        public Task<Respuesta> EnviarAprobacionComplementoAsync(UsuarioGeneral u, int idComplemento) =>
            EjecutarAsync("SP_InformeComplemento_EnviarAprobacion", u,
                cmd => cmd.Parameters.Add("@p_intIdComplemento", MySqlDbType.Int32).Value = idComplemento,
                async dr =>
                {
                    var resultado = new ComplementoEnviado();
                    if (await dr.NextResultAsync() && await dr.ReadAsync())
                        resultado.IdInforme = Entero(dr, "IdInforme");
                    return resultado;
                },
                new ComplementoEnviado());

        public Task<Respuesta> ListarClientesDestinoComplementoAsync(UsuarioGeneral u) =>
            EjecutarAsync("SP_InformeComplemento_ListarClientesDestino", u,
                _ => { },
                async dr =>
                {
                    var resultado = new ListaClientesDestinoComplemento();
                    if (!await dr.NextResultAsync()) return resultado;

                    while (await dr.ReadAsync())
                    {
                        resultado.lstCliente.Add(new ClienteDestinoComplemento
                        {
                            IdCliente = Entero(dr, "IdCliente"),
                            Nombre = Texto(dr, "Nombre")
                        });
                    }

                    return resultado;
                },
                new ListaClientesDestinoComplemento());

        // ── Infraestructura común ──────────────────────────────────────────────

        private async Task<Respuesta> EjecutarAsync<T>(
            string procedimiento,
            UsuarioGeneral u,
            Action<MySqlCommand> agregarParametros,
            Func<MySqlDataReader, Task<T>> leerResultado,
            T resultadoVacio)
        {
            try
            {
                using MySqlConnection cn = new(_dbConfig.ConnectionString);
                using MySqlCommand cmd = new(procedimiento, cn) { CommandType = CommandType.StoredProcedure };

                cmd.Parameters.Add("@p_intIdUsuario", MySqlDbType.Int32).Value = u.IdUsuario;
                cmd.Parameters.Add("@p_vchUsuario", MySqlDbType.VarChar, 32).Value = u.Usuario;
                cmd.Parameters.Add("@p_intIdEmpresa", MySqlDbType.Int32).Value = u.IdEmpresa;
                cmd.Parameters.Add("@p_intIdRol", MySqlDbType.Int32).Value = u.IdRol;
                agregarParametros(cmd);

                await cn.OpenAsync();

                using var dr = await cmd.ExecuteReaderAsync();
                var respuesta = await LeerCabeceraAsync(dr, procedimiento);

                respuesta.Result = respuesta.IdTipoMensaje == 2
                    ? await leerResultado(dr)
                    : resultadoVacio;

                return respuesta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la capa de datos.");

                return new Respuesta { IdTipoMensaje = 3, Mensaje = ex.Message, Result = resultadoVacio };
            }
        }

        // Lee el result set 1 (siempre presente): IdTipoMensaje, Mensaje.
        private async Task<Respuesta> LeerCabeceraAsync(MySqlDataReader dr, string procedimiento)
        {
            var respuesta = new Respuesta();

            if (await dr.ReadAsync())
            {
                respuesta.IdTipoMensaje = dr["IdTipoMensaje"] != DBNull.Value
                    ? Convert.ToInt32(dr["IdTipoMensaje"])
                    : 3;
                respuesta.Mensaje = dr["Mensaje"]?.ToString() ?? string.Empty;
            }
            else
            {
                _logger.LogWarning("El procedimiento {Procedimiento} no devolvio ninguna fila.", procedimiento);

                respuesta.IdTipoMensaje = 3;
                respuesta.Mensaje = "No se obtuvo respuesta del procedimiento.";
            }

            return respuesta;
        }

        private static object Nulo(string? valor) =>
            string.IsNullOrWhiteSpace(valor) ? DBNull.Value : valor.Trim();

        private static int Entero(MySqlDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? 0 : Convert.ToInt32(dr[columna]);

        private static int? EnteroNulo(MySqlDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : Convert.ToInt32(dr[columna]);

        private static string? Texto(MySqlDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : dr[columna].ToString();

        private static bool Booleano(MySqlDataReader dr, string columna) =>
            dr[columna] != DBNull.Value && Convert.ToInt32(dr[columna]) != 0;

        // Las fechas se guardan en UTC (NOW() del servidor / ToUniversalTime en los filtros):
        // se marcan como UTC para que el JSON salga con "Z" y el front las convierta a hora local.
        private static DateTime? FechaUtc(MySqlDataReader dr, string columna) =>
            dr[columna] == DBNull.Value ? null : DateTime.SpecifyKind(Convert.ToDateTime(dr[columna]), DateTimeKind.Utc);
    }
}
