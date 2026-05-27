using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace UrlShortener.Host.Swagger;

public sealed class AddHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.ApiDescription.RelativePath == null || !context.ApiDescription.RelativePath.StartsWith("api"))
        {
            return;
        }

        operation.Parameters ??= new List<OpenApiParameter>();

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-Api-Key",
            In = ParameterLocation.Header,
            Description = "API Key",
            Required = true,
            Schema = new OpenApiSchema
            {
                Type = "string"
            }
        });
    }
}
