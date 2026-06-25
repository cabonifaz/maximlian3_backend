using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafetyReport.Handlers;
using SafetyReport.Models;

namespace SafetyReport.WebApi.Controllers
{
    [Authorize]
    [Route("api/informeTranslation")]
    [ApiController]
    public class InformeTranslationController(InformeTranslationHandler handler) : BaseController
    {
        private readonly InformeTranslationHandler _handler = handler;

        [HttpPost("traducir")]
        public async Task<IActionResult> Traducir([FromBody] InformeTranslationRequest request)
        {
            var respuesta = await _handler.TranslateAsync(request);
            return Ok(respuesta);
        }
    }
}
