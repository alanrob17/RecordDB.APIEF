using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using RecordDB.APIEF.DTOs;
using RecordDB.APIEF.Models;
using RecordModel = RecordDB.APIEF.Models.Record;

namespace RecordDB.APIEF.Tests.IntegrationTests
{
    public class RecordEndpointTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public RecordEndpointTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
            _factory.ResetMocks();
        }

        [Fact]
        public async Task GetAll_ReturnsOkWithRecordList()
        {
            // Arrange
            var records = new List<ArtistRecordDto>
            {
                new() { RecordId = 1, Name = "Kind of Blue", ArtistName = "Miles Davis" },
                new() { RecordId = 2, Name = "Giant Steps", ArtistName = "John Coltrane" }
            };
            _factory.RecordRepoMock.Setup(r => r.SelectAsync()).ReturnsAsync(records);

            // Act
            var response = await _client.GetAsync("/api/record");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var dtos = await response.Content.ReadFromJsonAsync<List<ArtistRecordDto>>();
            dtos.Should().NotBeNull();
            dtos!.Should().HaveCount(2);
            dtos[0].Name.Should().Be("Kind of Blue");
        }

        [Fact]
        public async Task GetById_Existing_ReturnsOk()
        {
            // Arrange
            var record = new ArtistRecordDto { RecordId = 1, Name = "Kind of Blue", ArtistName = "Miles Davis" };
            _factory.RecordRepoMock.Setup(r => r.SelectAsync(1)).ReturnsAsync(record);

            // Act
            var response = await _client.GetAsync("/api/record/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var dto = await response.Content.ReadFromJsonAsync<ArtistRecordDto>();
            dto.Should().NotBeNull();
            dto!.RecordId.Should().Be(1);
            dto.Name.Should().Be("Kind of Blue");
        }

        [Fact]
        public async Task GetById_NotFound_Returns404()
        {
            // Arrange
            _factory.RecordRepoMock.Setup(r => r.SelectAsync(999)).ReturnsAsync((ArtistRecordDto?)null);

            // Act
            var response = await _client.GetAsync("/api/record/999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetByArtistId_ReturnsOkWithFlatRecordList()
        {
            // Arrange
            var records = new List<RecordModel>
            {
                new() { RecordId = 1, ArtistId = 42, Name = "Kind of Blue", Recorded = 1959 }
            };
            _factory.RecordRepoMock.Setup(r => r.GetArtistRecordsAsync(42)).ReturnsAsync(records);

            // Act
            var response = await _client.GetAsync("/api/record/by-artist-id/42");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<List<RecordModel>>();
            result.Should().NotBeNull();
            result!.Should().ContainSingle();
            result[0].Name.Should().Be("Kind of Blue");
        }

        [Fact]
        public async Task GetReviews_ReturnsOkWithReviewList()
        {
            // Arrange
            var reviews = new List<RecordReviewDto>
            {
                new() { Name = "Miles Davis", Title = "Kind of Blue", Review = "Essential masterpiece" }
            };
            _factory.RecordRepoMock.Setup(r => r.SelectRecordReviewsAsync()).ReturnsAsync(reviews);

            // Act
            var response = await _client.GetAsync("/api/record/reviews");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var dtos = await response.Content.ReadFromJsonAsync<List<RecordReviewDto>>();
            dtos.Should().NotBeNull();
            dtos!.Should().ContainSingle();
            dtos[0].Review.Should().Be("Essential masterpiece");
        }

        [Fact]
        public async Task Create_ValidRecord_ReturnsCreatedWithLocationHeader()
        {
            // Arrange
            var newRecord = new CreateRecordDto
            {
                ArtistId = 1,
                Name = "Sketches of Spain",
                Field = "Jazz",
                Discs = 1,
                Cost = 19.99m
            };
            _factory.RecordRepoMock.Setup(r => r.InsertAsync(It.IsAny<RecordModel>())).ReturnsAsync(200);

            // Act
            var response = await _client.PostAsJsonAsync("/api/record", newRecord);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();
            response.Headers.Location!.ToString().Should().Contain("/api/Record/200");

            var createdId = await response.Content.ReadFromJsonAsync<int>();
            createdId.Should().Be(200);
        }

        [Fact]
        public async Task Update_ValidUpdate_ReturnsNoContent()
        {
            // Arrange
            var existing = new ArtistRecordDto { RecordId = 10, Name = "Old" };
            var updateDto = new UpdateRecordDto { RecordId = 10, Name = "Updated Title" };

            _factory.RecordRepoMock.Setup(r => r.SelectAsync(10)).ReturnsAsync(existing);
            _factory.RecordRepoMock.Setup(r => r.UpdateAsync(It.IsAny<ArtistRecordDto>())).ReturnsAsync(1);

            // Act
            var response = await _client.PutAsJsonAsync("/api/record/10", updateDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Delete_Existing_ReturnsNoContent()
        {
            // Arrange
            var existing = new ArtistRecordDto { RecordId = 10 };
            _factory.RecordRepoMock.Setup(r => r.SelectAsync(10)).ReturnsAsync(existing);
            _factory.RecordRepoMock.Setup(r => r.DeleteAsync(10)).Returns(Task.CompletedTask);

            // Act
            var response = await _client.DeleteAsync("/api/record/10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }
    }
}
