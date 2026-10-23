using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using RecordDB.APIEF.DTOs;
using RecordDB.APIEF.Models;

namespace RecordDB.APIEF.Tests.IntegrationTests
{
    public class DiscEndpointTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public DiscEndpointTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
            _factory.ResetMocks();
        }

        [Fact]
        public async Task GetAll_ReturnsOkWithDiscList()
        {
            // Arrange
            var discs = new List<ArtistRecordDiscDto>
            {
                new() { DiscId = 1, RecordId = 1, DiscNo = 1, Name = "Kind of Blue" },
                new() { DiscId = 2, RecordId = 1, DiscNo = 2, Name = "Kind of Blue" }
            };
            _factory.DiscRepoMock.Setup(r => r.SelectAllDiscEntitiesAsync()).ReturnsAsync(discs);

            // Act
            var response = await _client.GetAsync("/api/disc");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var dtos = await response.Content.ReadFromJsonAsync<List<ArtistRecordDiscDto>>();
            dtos.Should().NotBeNull();
            dtos!.Should().HaveCount(2);
            dtos[0].DiscId.Should().Be(1);
        }

        [Fact]
        public async Task GetById_Existing_ReturnsOk()
        {
            // Arrange
            var disc = new ArtistRecordDiscDto { DiscId = 10, RecordId = 5, DiscNo = 1, Name = "Album" };
            _factory.DiscRepoMock.Setup(r => r.SelectSingleDiscAsync(10)).ReturnsAsync(disc);

            // Act
            var response = await _client.GetAsync("/api/disc/10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var dto = await response.Content.ReadFromJsonAsync<ArtistRecordDiscDto>();
            dto.Should().NotBeNull();
            dto!.DiscId.Should().Be(10);
        }

        [Fact]
        public async Task GetById_NotFound_Returns404()
        {
            // Arrange
            _factory.DiscRepoMock.Setup(r => r.SelectSingleDiscAsync(999)).ReturnsAsync((ArtistRecordDiscDto?)null);

            // Act
            var response = await _client.GetAsync("/api/disc/999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Create_ValidDisc_ReturnsCreated()
        {
            // Arrange
            var createDto = new CreateDiscDto
            {
                RecordId = 5,
                DiscNo = 1,
                Length = 2400
            };
            _factory.DiscRepoMock.Setup(r => r.InsertDiscAsync(It.IsAny<Disc>())).ReturnsAsync(15);

            // Act
            var response = await _client.PostAsJsonAsync("/api/disc", createDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();
            response.Headers.Location!.ToString().Should().Contain("/api/Disc/15");

            var createdId = await response.Content.ReadFromJsonAsync<int>();
            createdId.Should().Be(15);
        }

        [Fact]
        public async Task PatchLength_ExistingDisc_ReturnsNoContent()
        {
            // Arrange
            var existing = new ArtistRecordDiscDto { DiscId = 10 };
            _factory.DiscRepoMock.Setup(r => r.SelectSingleDiscAsync(10)).ReturnsAsync(existing);
            _factory.DiscRepoMock.Setup(r => r.UpdateDiscLengthAsync(10, 2800)).Returns(Task.CompletedTask);

            // Act
            var response = await _client.PatchAsJsonAsync("/api/disc/10/length", new UpdateDiscLengthDto { Length = 2800 });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Delete_Existing_ReturnsNoContent()
        {
            // Arrange
            var existing = new ArtistRecordDiscDto { DiscId = 10 };
            _factory.DiscRepoMock.Setup(r => r.SelectSingleDiscAsync(10)).ReturnsAsync(existing);
            _factory.DiscRepoMock.Setup(r => r.DeleteDiscAsync(10)).Returns(Task.CompletedTask);

            // Act
            var response = await _client.DeleteAsync("/api/disc/10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }
    }
}
