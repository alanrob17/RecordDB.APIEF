using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RecordDB.APIEF.Controllers;
using RecordDB.APIEF.DTOs;
using RecordDB.APIEF.Models;
using RecordDB.APIEF.Repositories;

namespace RecordDB.APIEF.Tests.UnitTests.Controllers
{
    public class TrackControllerTests
    {
        private readonly Mock<ITrackRepository> _mockRepo;
        private readonly TrackController _controller;

        public TrackControllerTests()
        {
            _mockRepo = new Mock<ITrackRepository>();
            _controller = new TrackController(_mockRepo.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOkWithTracks()
        {
            // Arrange
            var tracks = new List<ArtistRecordDiscTrackDto>
            {
                new() { TrackId = 1, Name = "So What", DiscId = 1, TrackNo = 1 },
                new() { TrackId = 2, Name = "Freddie Freeloader", DiscId = 1, TrackNo = 2 }
            };
            _mockRepo.Setup(r => r.SelectAllTrackEntitiesAsync()).ReturnsAsync(tracks);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var dtos = okResult.Value.Should().BeAssignableTo<IEnumerable<ArtistRecordDiscTrackDto>>().Subject.ToList();
            dtos.Should().HaveCount(2);
            dtos[0].Name.Should().Be("So What");
        }

        [Fact]
        public async Task GetById_Existing_ReturnsOk()
        {
            // Arrange
            var track = new ArtistRecordDiscTrackDto { TrackId = 5, Name = "Blue in Green" };
            _mockRepo.Setup(r => r.SelectTrackByIdAsync(5)).ReturnsAsync(track);

            // Act
            var result = await _controller.GetById(5);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var dto = okResult.Value.Should().BeOfType<ArtistRecordDiscTrackDto>().Subject;
            dto.TrackId.Should().Be(5);
            dto.Name.Should().Be("Blue in Green");
        }

        [Fact]
        public async Task GetById_NotFound_ReturnsNotFound()
        {
            // Arrange
            _mockRepo.Setup(r => r.SelectTrackByIdAsync(99)).ReturnsAsync((ArtistRecordDiscTrackDto?)null);

            // Act
            var result = await _controller.GetById(99);

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Search_ReturnsMatchingTracks()
        {
            // Arrange
            var tracks = new List<ArtistRecordDiscTrackDto>
            {
                new() { TrackId = 1, Name = "All Blues" }
            };
            _mockRepo.Setup(r => r.SelectTracksByPartialNameAsync("Blues")).ReturnsAsync(tracks);

            // Act
            var result = await _controller.Search("Blues");

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var dtos = okResult.Value.Should().BeAssignableTo<IEnumerable<ArtistRecordDiscTrackDto>>().Subject.ToList();
            dtos.Should().ContainSingle();
            dtos[0].Name.Should().Be("All Blues");
        }

        [Fact]
        public async Task GetByRecord_ReturnsTracks()
        {
            // Arrange
            var tracks = new List<ArtistRecordDiscTrackDto>
            {
                new() { TrackId = 1, Name = "So What" }
            };
            _mockRepo.Setup(r => r.SelectTracksByRecordAsync("Kind of Blue")).ReturnsAsync(tracks);

            // Act
            var result = await _controller.GetByRecord("Kind of Blue");

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var dtos = okResult.Value.Should().BeAssignableTo<IEnumerable<ArtistRecordDiscTrackDto>>().Subject.ToList();
            dtos.Should().ContainSingle();
        }

        [Fact]
        public async Task GetByArtist_ReturnsTracks()
        {
            // Arrange
            var tracks = new List<ArtistRecordDiscTrackDto>
            {
                new() { TrackId = 1, ArtistName = "Miles Davis", Name = "Flamenco Sketches" }
            };
            _mockRepo.Setup(r => r.SelectArtistRecordTracksAsync("Miles Davis")).ReturnsAsync(tracks);

            // Act
            var result = await _controller.GetByArtist("Miles Davis");

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var dtos = okResult.Value.Should().BeAssignableTo<IEnumerable<ArtistRecordDiscTrackDto>>().Subject.ToList();
            dtos.Should().ContainSingle();
            dtos[0].Name.Should().Be("Flamenco Sketches");
        }

        [Fact]
        public async Task Create_Successful_ReturnsCreatedAtActionWithId()
        {
            // Arrange
            var createDto = new CreateTrackDto
            {
                DiscId = 1,
                TrackNo = 1,
                Name = "Intro",
                TrackLength = 120
            };
            _mockRepo.Setup(r => r.InsertTrackAsync(It.Is<Track>(t => t.DiscId == 1 && t.Name == "Intro")))
                     .ReturnsAsync(55);

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.ActionName.Should().Be(nameof(TrackController.GetById));
            createdResult.RouteValues!["id"].Should().Be(55);
            createdResult.Value.Should().Be(55);
        }

        [Fact]
        public async Task Create_Failed_ReturnsBadRequest()
        {
            // Arrange
            var createDto = new CreateTrackDto { DiscId = 1, TrackNo = 1, Name = "Fail" };
            _mockRepo.Setup(r => r.InsertTrackAsync(It.IsAny<Track>())).ReturnsAsync(0);

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var badRequest = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().Be("Track could not be created.");
        }

        [Fact]
        public async Task BulkCreate_EmptyList_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.BulkCreate(new List<CreateTrackDto>());

            // Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().Be("Track collection cannot be empty.");
        }

        [Fact]
        public async Task BulkCreate_ValidList_ReturnsNoContent()
        {
            // Arrange
            var dtos = new List<CreateTrackDto>
            {
                new() { DiscId = 1, TrackNo = 1, Name = "Track 1" },
                new() { DiscId = 1, TrackNo = 2, Name = "Track 2" }
            };
            _mockRepo.Setup(r => r.BulkInsertTracksAsync(It.IsAny<IEnumerable<Track>>())).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.BulkCreate(dtos);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            _mockRepo.Verify(r => r.BulkInsertTracksAsync(It.Is<IEnumerable<Track>>(list => list.Count() == 2)), Times.Once);
        }

        [Fact]
        public async Task Update_IdMismatch_ReturnsBadRequest()
        {
            // Arrange
            var updateDto = new UpdateTrackDto { TrackId = 10, DiscId = 1, TrackNo = 1, Name = "Test" };

            // Act
            var result = await _controller.Update(5, updateDto);

            // Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().Be("Route id does not match body TrackId.");
        }

        [Fact]
        public async Task Update_NotFound_ReturnsNotFound()
        {
            // Arrange
            var updateDto = new UpdateTrackDto { TrackId = 99, DiscId = 1, TrackNo = 1, Name = "Test" };
            _mockRepo.Setup(r => r.SelectTrackByIdAsync(99)).ReturnsAsync((ArtistRecordDiscTrackDto?)null);

            // Act
            var result = await _controller.Update(99, updateDto);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Update_Existing_ReturnsNoContent()
        {
            // Arrange
            var existing = new ArtistRecordDiscTrackDto { TrackId = 10, Name = "Old" };
            var updateDto = new UpdateTrackDto { TrackId = 10, DiscId = 1, TrackNo = 1, Name = "New" };
            _mockRepo.Setup(r => r.SelectTrackByIdAsync(10)).ReturnsAsync(existing);
            _mockRepo.Setup(r => r.UpdateTrackAsync(It.IsAny<Track>())).ReturnsAsync(1);

            // Act
            var result = await _controller.Update(10, updateDto);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            _mockRepo.Verify(r => r.UpdateTrackAsync(It.Is<Track>(t => t.TrackId == 10 && t.Name == "New")), Times.Once);
        }

        [Fact]
        public async Task Delete_NotFound_ReturnsNotFound()
        {
            // Arrange
            _mockRepo.Setup(r => r.SelectTrackByIdAsync(99)).ReturnsAsync((ArtistRecordDiscTrackDto?)null);

            // Act
            var result = await _controller.Delete(99);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Delete_Existing_ReturnsNoContent()
        {
            // Arrange
            var existing = new ArtistRecordDiscTrackDto { TrackId = 10 };
            _mockRepo.Setup(r => r.SelectTrackByIdAsync(10)).ReturnsAsync(existing);
            _mockRepo.Setup(r => r.DeleteTrackAsync(10)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Delete(10);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            _mockRepo.Verify(r => r.DeleteTrackAsync(10), Times.Once);
        }
    }
}
