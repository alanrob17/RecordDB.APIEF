using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using RecordDB.APIEF.DTOs;
using RecordDB.APIEF.Models;

namespace RecordDB.APIEF.Tests.IntegrationTests
{
    public class TrackEndpointTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public TrackEndpointTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
            _factory.ResetMocks();
        }

        [Fact]
        public async Task GetAll_ReturnsOkWithTrackList()
        {
            // Arrange
            var tracks = new List<ArtistRecordDiscTrackDto>
            {
                new() { TrackId = 1, Name = "So What", DiscId = 1, TrackNo = 1 },
                new() { TrackId = 2, Name = "Freddie Freeloader", DiscId = 1, TrackNo = 2 }
            };
            _factory.TrackRepoMock.Setup(r => r.SelectAllTrackEntitiesAsync()).ReturnsAsync(tracks);

            // Act
            var response = await _client.GetAsync("/api/track");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var dtos = await response.Content.ReadFromJsonAsync<List<ArtistRecordDiscTrackDto>>();
            dtos.Should().NotBeNull();
            dtos!.Should().HaveCount(2);
            dtos[0].Name.Should().Be("So What");
        }

        [Fact]
        public async Task GetById_Existing_ReturnsOk()
        {
            // Arrange
            var track = new ArtistRecordDiscTrackDto { TrackId = 1, Name = "So What", DiscId = 1, TrackNo = 1 };
            _factory.TrackRepoMock.Setup(r => r.SelectTrackByIdAsync(1)).ReturnsAsync(track);

            // Act
            var response = await _client.GetAsync("/api/track/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var dto = await response.Content.ReadFromJsonAsync<ArtistRecordDiscTrackDto>();
            dto.Should().NotBeNull();
            dto!.TrackId.Should().Be(1);
            dto.Name.Should().Be("So What");
        }

        [Fact]
        public async Task GetById_NotFound_Returns404()
        {
            // Arrange
            _factory.TrackRepoMock.Setup(r => r.SelectTrackByIdAsync(999)).ReturnsAsync((ArtistRecordDiscTrackDto?)null);

            // Act
            var response = await _client.GetAsync("/api/track/999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Create_ValidTrack_ReturnsCreated()
        {
            // Arrange
            var createDto = new CreateTrackDto
            {
                DiscId = 1,
                TrackNo = 1,
                Name = "Blue in Green",
                TrackLength = 327
            };
            _factory.TrackRepoMock.Setup(r => r.InsertTrackAsync(It.IsAny<Track>())).ReturnsAsync(50);

            // Act
            var response = await _client.PostAsJsonAsync("/api/track", createDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();
            response.Headers.Location!.ToString().Should().Contain("/api/Track/50");

            var createdId = await response.Content.ReadFromJsonAsync<int>();
            createdId.Should().Be(50);
        }

        [Fact]
        public async Task BulkCreate_ValidList_ReturnsNoContent()
        {
            // Arrange
            var batch = new List<CreateTrackDto>
            {
                new() { DiscId = 1, TrackNo = 1, Name = "Track 1" },
                new() { DiscId = 1, TrackNo = 2, Name = "Track 2" }
            };
            _factory.TrackRepoMock.Setup(r => r.BulkInsertTracksAsync(It.IsAny<IEnumerable<Track>>())).Returns(Task.CompletedTask);

            // Act
            var response = await _client.PostAsJsonAsync("/api/track/bulk", batch);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Update_ValidUpdate_ReturnsNoContent()
        {
            // Arrange
            var existing = new ArtistRecordDiscTrackDto { TrackId = 10, Name = "Old Title" };
            var updateDto = new UpdateTrackDto { TrackId = 10, DiscId = 1, TrackNo = 1, Name = "New Title" };

            _factory.TrackRepoMock.Setup(r => r.SelectTrackByIdAsync(10)).ReturnsAsync(existing);
            _factory.TrackRepoMock.Setup(r => r.UpdateTrackAsync(It.IsAny<Track>())).ReturnsAsync(1);

            // Act
            var response = await _client.PutAsJsonAsync("/api/track/10", updateDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Delete_Existing_ReturnsNoContent()
        {
            // Arrange
            var existing = new ArtistRecordDiscTrackDto { TrackId = 10 };
            _factory.TrackRepoMock.Setup(r => r.SelectTrackByIdAsync(10)).ReturnsAsync(existing);
            _factory.TrackRepoMock.Setup(r => r.DeleteTrackAsync(10)).Returns(Task.CompletedTask);

            // Act
            var response = await _client.DeleteAsync("/api/track/10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }
    }
}
