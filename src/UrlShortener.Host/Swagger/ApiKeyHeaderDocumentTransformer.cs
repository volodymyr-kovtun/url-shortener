using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace UrlShortener.Host.Swagger;

internal sealed class ApiKeyHeaderDocumentTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        foreach (var (path, item) in document.Paths)
        {
            if (!path.StartsWith("/api", StringComparison.Ordinal) || item.Operations is null)
            {
                continue;
            }

            foreach (var operation in item.Operations.Values)
            {
                operation.Parameters ??= [];
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "X-Api-Key",
                    In = ParameterLocation.Header,
                    Description = "API Key",
                    Required = true,
                    Schema = new OpenApiSchema { Type = JsonSchemaType.String },
                });
            }
        }

        return Task.CompletedTask;
    }
}
