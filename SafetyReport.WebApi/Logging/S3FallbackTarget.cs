using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using NLog.Targets;

namespace SafetyReport.WebApi.Logging
{
    [Target("S3Fallback")]
    public sealed class S3FallbackTarget : AsyncTaskTarget
    {
        public static IS3UploadService? UploadService { get; set; }

        public S3FallbackTarget()
        {
            Layout = "${longdate}|${level:uppercase=true}|${callsite}|${aspnet-user-identity}|${message}${onexception:${newline}${exception:format=tostring}}";
        }

        protected override async Task WriteAsyncTask(LogEventInfo logEvent, CancellationToken token)
        {
            var s3 = UploadService;
            if (s3 is null)
                throw new InvalidOperationException("S3FallbackTarget.UploadService no fue inicializado todavía.");

            var rutaArchivo = $"logs/errores-no-controlados/{logEvent.TimeStamp:yyyy-MM-dd}.txt";
            var linea = RenderLogEvent(Layout, logEvent);

            var existente = await s3.DescargarBytesAsync(rutaArchivo);
            var contenidoPrevio = existente is not null ? Encoding.UTF8.GetString(existente) : string.Empty;
            var contenidoNuevo = contenidoPrevio + linea + Environment.NewLine;

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(contenidoNuevo));
            await s3.UploadStreamAsync(rutaArchivo, stream, "text/plain");
        }
    }
}
