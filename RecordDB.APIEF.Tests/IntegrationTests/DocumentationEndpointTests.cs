using System.Net;
using FluentAssertions;

namespace RecordDB.APIEF.Tests.IntegrationTests
{
    public class DocumentationEndpointTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public DocumentationEndpointTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        public async Task OpenApiJson_ReturnsOkAndValidSpecification()
        {
            // Act
            var response = await _client.GetAsync("/openapi/v1.json");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType!.MediaType.Should().Be("application/json");

            var json = await response.Content.ReadAsStringAsync();
            json.Should().Contain("\"openapi\"");
            json.Should().Contain("\"paths\"");
            json.Should().Contain("/api/Artist");
            json.Should().Contain("/api/Record");
            json.Should().Contain("/api/Disc");
            json.Should().Contain("/api/Track");
        }

        [Fact]
        public async Task SwaggerUi_ReturnsSuccessStatusCode()
        {
            // Act
            var response = await _client.GetAsync("/swagger/index.html");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("swagger");
        }

        [Fact]
        public async Task ScalarUi_ReturnsSuccessStatusCode()
        {
            // Act
            var response = await _client.GetAsync("/scalar/v1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("scalar.js");
            content.Should().Contain("RecordDB API Reference (EF Core)");
        }

        [Fact]
        public async Task ReDocUi_ReturnsSuccessStatusCode()
        {
            // Act
            var response = await _client.GetAsync("/redoc/index.html");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("redoc");
        }

        [Fact]
        public async Task Root_RedirectsToScalarV1()
        {
            // Act
            var response = await _client.GetAsync("/");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location!.OriginalString.Should().Be("/scalar/v1");
        }
    }
}
