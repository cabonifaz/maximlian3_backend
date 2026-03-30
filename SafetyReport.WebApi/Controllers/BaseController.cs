using Microsoft.AspNetCore.Mvc;
using SafetyReport.Models;
using SafetyReport.WebApi.Helpers;

namespace SafetyReport.WebApi.Controllers
{
    [ApiController]
    public class BaseController : ControllerBase
    {
        protected UsuarioGeneral UsuarioLogueado => TokenHelper.GetUsuario(User, Request);
    }
}
