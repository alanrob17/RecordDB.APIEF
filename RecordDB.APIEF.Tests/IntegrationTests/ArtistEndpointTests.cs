using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using RecordDB.APIEF.DTOs;
using RecordDB.APIEF.Models;

namespace RecordDB.APIEF.Tests.IntegrationTests
{
    public class ArtistEndpointTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public ArtistEndpointTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
            _factory.ResetMocks();
        }

        [Fact]
        public async Task GetAll_ReturnsOkWithArtistList()
        {
            // Arrange
            var artists = new List<Artist>
            {
                new() { ArtistId = 1, Name = "Miles Davis", FirstName = "Miles", LastName = "Davis" },
                new() { ArtistId = 2, Name = "John Coltrane", FirstName = "John", LastName = "Coltrane" }
            };
            _factory.ArtistRepoMock.Setup(r => r.GetArtistsAsync()).ReturnsAsync(artists);

            // Act
            var response = await _client.GetAsync("/api/artist");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var dtos = await response.Content.ReadFromJsonAsync<List<ArtistDto>>();
            dtos.Should().NotBeNull();
            dtos!.Should().HaveCount(2);
            dtos[0].Name.Should().Be("Miles Davis");
        }

        [Fact]
        public async Task GetById_Existing_ReturnsOkWithArtist()
        {
            // Arrange
            var artist = new Artist { ArtistId = 1, Name = "Miles Davis", FirstName = "Miles", LastName = "Davis", Biography = "Jazz legend" };
            _factory.ArtistRepoMock.Setup(r => r.SelectAsync(1)).ReturnsAsync(artist);

            // Act
            var response = await _client.GetAsync("/api/artist/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var dto = await response.Content.ReadFromJsonAsync<ArtistDto>();
            dto.Should().NotBeNull();
            dto!.ArtistId.Should().Be(1);
            dto.Name.Should().Be("Miles Davis");
        }

        [Fact]
        public async Task GetById_NotFound_Returns404()
        {
            // Arrange
            _factory.ArtistRepoMock.Setup(r => r.SelectAsync(999)).ReturnsAsync((Artist?)null);

            // Act
            var response = await _client.GetAsync("/api/artist/999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Create_ValidArtist_ReturnsCreatedWithLocationHeader()
        {
            // Arrange
            var newArtist = new CreateArtistDto
            {
                FirstName = "Herbie",
                LastName = "Hancock",
                Biography = "Jazz Pianist"
            };
            _factory.ArtistRepoMock.Setup(r => r.InsertAsync(It.IsAny<Artist>())).ReturnsAsync(101);

            // Act
            var response = await _client.PostAsJsonAsync("/api/artist", newArtist);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();
            response.Headers.Location!.ToString().Should().Contain("/api/Artist/101");

            var createdId = await response.Content.ReadFromJsonAsync<int>();
            createdId.Should().Be(101);
        }

        [Fact]
        public async Task Update_ValidUpdate_ReturnsNoContent()
        {
            // Arrange
            var existing = new Artist { ArtistId = 5, FirstName = "Old", Name = "Old" };
            var updateDto = new UpdateArtistDto { ArtistId = 5, FirstName = "New", Name = "New" };

            _factory.ArtistRepoMock.Setup(r => r.SelectAsync(5)).ReturnsAsync(existing);
            _factory.ArtistRepoMock.Setup(r => r.UpdateArtistAsync(It.IsAny<Artist>())).ReturnsAsync(1);

            // Act
            var response = await _client.PutAsJsonAsync("/api/artist/5", updateDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Delete_Existing_ReturnsNoContent()
        {
            // Arrange
            var existing = new Artist { ArtistId = 5 };
            _factory.ArtistRepoMock.Setup(r => r.SelectAsync(5)).ReturnsAsync(existing);
            _factory.ArtistRepoMock.Setup(r => r.DeleteAsync(5)).Returns(Task.CompletedTask);

            // Act
            var response = await _client.DeleteAsync("/api/artist/5");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }
    }
}
