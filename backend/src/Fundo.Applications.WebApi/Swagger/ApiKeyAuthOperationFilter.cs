using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;

namespace Fundo.Applications.WebApi.Swagger
{
    public class ApiKeyAuthOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var hasAuthorize = (context.MethodInfo.DeclaringType?.GetCustomAttributes(true)
                    .OfType<AuthorizeAttribute>()
                    .Any() == true)
                || context.MethodInfo.GetCustomAttributes(true)
                    .OfType<AuthorizeAttribute>()
                    .Any();

            if (!hasAuthorize)
            {
                return;
            }

            operation.Security ??= new List<OpenApiSecurityRequirement>();

            var scheme = new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            };

            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [scheme] = new List<string>()
            });
        }
    }
}
