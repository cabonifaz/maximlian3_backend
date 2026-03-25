using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SafetyReport.WebApi.Helpers
{
    public class SwaggerHeaderFilter : IOperationFilter
    {
        private static readonly (string Name, string Description)[] CustomHeaders =
        [
            ("idUsuario",  "ID del usuario autenticado"),
            ("idEmpresa",  "ID de la empresa del usuario"),
            ("idRol",      "ID del rol activo del usuario")
        ];

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var hasAllowAnonymous = context.MethodInfo
                .GetCustomAttributes(true)
                .OfType<AllowAnonymousAttribute>()
                .Any()
                || (context.MethodInfo.DeclaringType?
                    .GetCustomAttributes(true)
                    .OfType<AllowAnonymousAttribute>()
                    .Any() ?? false);

            if (hasAllowAnonymous)
                return;

            foreach (var (name, description) in CustomHeaders)
            {
                operation.Parameters!.Add(new OpenApiParameter
                {
                    Name = name,
                    In = ParameterLocation.Header,
                    Required = true,
                    Description = description,
                    Schema = new OpenApiSchema { Type = JsonSchemaType.Integer }
                });
            }
        }
    }
}
