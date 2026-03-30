using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SafetyReport.WebApi.Helpers
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class N8nHeaderAttribute : Attribute { }

    public class SwaggerHeaderFilter : IOperationFilter
    {
        private static readonly (string Name, string Description, bool IsString)[] CustomHeaders =
        [
            ("idUsuario",  "ID del usuario autenticado",       false),
            ("idEmpresa",  "ID de la empresa del usuario",     false),
            ("idRol",      "ID del rol activo del usuario",    false)
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

            foreach (var (name, description, isString) in CustomHeaders)
            {
                operation.Parameters!.Add(new OpenApiParameter
                {
                    Name = name,
                    In = ParameterLocation.Header,
                    Required = true,
                    Description = description,
                    Schema = new OpenApiSchema { Type = isString ? JsonSchemaType.String : JsonSchemaType.Integer }
                });
            }

            var hasN8nHeader = context.MethodInfo
                .GetCustomAttributes(true)
                .OfType<N8nHeaderAttribute>()
                .Any()
                || (context.MethodInfo.DeclaringType?
                    .GetCustomAttributes(true)
                    .OfType<N8nHeaderAttribute>()
                    .Any() ?? false);

            if (hasN8nHeader)
            {
                operation.Parameters!.Add(new OpenApiParameter
                {
                    Name = "username",
                    In = ParameterLocation.Header,
                    Required = true,
                    Description = "Username de Cognito (requerido para clientes N8n)",
                    Schema = new OpenApiSchema { Type = JsonSchemaType.String }
                });
            }
        }
    }
}
