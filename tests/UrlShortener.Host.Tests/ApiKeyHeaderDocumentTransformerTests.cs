using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using UrlShortener.Host.Swagger;

namespace UrlShortener.Host.Tests;

public sealed class ApiKeyHeaderDocumentTransformerTests
{
    [Fact]
    public async Task TransformAsync_GivenApiPath_AddsXApiKeyHeaderParameter()
    {
        //Arrange
        var ct = TestContext.Current.CancellationToken;
        var transformer = new ApiKeyHeaderDocumentTransformer();
        var document = BuildDocumentWithPath("/api/shorten");

        //Act
        await transformer.TransformAsync(document, context: null!, ct);

        //Assert
        var operation = document.Paths["/api/shorten"].Operations!.Values.Single();
        operation.Parameters.ShouldNotBeNull();
        operation.Parameters.ShouldContain(p =>
            p.Name == "X-Api-Key" && p.In == ParameterLocation.Header && p.Required == true);
    }

    [Fact]
    public async Task TransformAsync_GivenNonApiPath_DoesNotAddHeader()
    {
        //Arrange
        var ct = TestContext.Current.CancellationToken;
        var transformer = new ApiKeyHeaderDocumentTransformer();
        var document = BuildDocumentWithPath("/health");

        //Act
        await transformer.TransformAsync(document, context: null!, ct);

        //Assert
        var operation = document.Paths["/health"].Operations!.Values.Single();
        (operation.Parameters ?? []).ShouldNotContain(p => p.Name == "X-Api-Key");
    }

    [Fact]
    public async Task TransformAsync_GivenExistingParameters_PreservesThem()
    {
        //Arrange
        var ct = TestContext.Current.CancellationToken;
        var transformer = new ApiKeyHeaderDocumentTransformer();
        var document = BuildDocumentWithPath("/api/shorten");
        var operation = document.Paths["/api/shorten"].Operations!.Values.Single();
        operation.Parameters =
        [
            new OpenApiParameter { Name = "existing", In = ParameterLocation.Query },
        ];

        //Act
        await transformer.TransformAsync(document, context: null!, ct);

        //Assert
        operation.Parameters.ShouldContain(p => p.Name == "existing");
        operation.Parameters.ShouldContain(p => p.Name == "X-Api-Key");
    }

    [Fact]
    public async Task TransformAsync_GivenPathWithoutOperations_DoesNotThrow()
    {
        //Arrange
        var ct = TestContext.Current.CancellationToken;
        var transformer = new ApiKeyHeaderDocumentTransformer();
        var document = new OpenApiDocument
        {
            Paths = new OpenApiPaths
            {
                ["/api/empty"] = new OpenApiPathItem(),
            },
        };

        //Act
        var act = async () => await transformer.TransformAsync(document, context: null!, ct);

        //Assert
        await act.ShouldNotThrowAsync();
    }

    private static OpenApiDocument BuildDocumentWithPath(string path)
    {
        return new OpenApiDocument
        {
            Paths = new OpenApiPaths
            {
                [path] = new OpenApiPathItem
                {
                    Operations = new Dictionary<HttpMethod, OpenApiOperation>
                    {
                        [HttpMethod.Post] = new OpenApiOperation(),
                    },
                },
            },
        };
    }
}
