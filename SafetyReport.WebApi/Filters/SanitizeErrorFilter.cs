using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SafetyReport.Models;

namespace SafetyReport.WebApi.Filters
{
    public class SanitizeErrorFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var executed = await next();
            if (executed.Result is OkObjectResult { Value: Respuesta r } && r.IdTipoMensaje == 3)
                r.Mensaje = "Error interno del servidor.";
        }
    }
}
