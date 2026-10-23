using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RecordDB.APIEF.Controllers;
using RecordDB.APIEF.DTOs;
using RecordDB.APIEF.Models;
using RecordDB.APIEF.Repositories;

namespace RecordDB.APIEF.Tests.UnitTests.Controllers
{
    public class ArtistControllerTests
    {
        private readonly Mock<IArtistRepository> _mockRepo;
        private readonly ArtistController _controller;

        public ArtistControllerTests()
        {
            _mockRepo = new Mock<IArtistRepository>();
            _controller = new ArtistController(_mockRepo.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOkWithArtistDtos()
        {
            // Arrange
            var artists = new List<Artist>
            {
                new() { ArtistId = 1, FirstName = "Miles", LastName = "Davis", Name = "Miles Davis", Biography = "Jazz legend" },
                new() { ArtistId = 2, FirstName = "John", LastName = "Coltrane", Name = "John Coltrane", Biography = "Saxophonist" }
            };
            _mockRepo.Setup(r => r.GetArtistsAsync()).ReturnsAsync(artists);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var dtos = okResult.Value.Should().BeAssignableTo<IEnumerable<ArtistDto>>().Subject.ToList();
            dtos.Should().HaveCount(2);
            dtos[0].Name.Should().Be("Miles Davis");
            dtos[1].Name.Should().Be("John Coltrane");
        }

        [Fact]
        public async Task GetById_ExistingId_ReturnsOkWithArtistDto()
        {
            // Arrange
            var artist = new Artist { ArtistId = 42, FirstName = "Bill", LastName = "Evans", Name = "Bill Evans", Biography = "Pianist" };
            _mockRepo.Setup(r => r.SelectAsync(42)).ReturnsAsync(artist);

            // Act
            var result = await _controller.GetById(42);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var dto = okResult.Value.Should().BeOfType<ArtistDto>().Subject;
            dto.ArtistId.Should().Be(42);
            dto.Name.Should().Be("Bill Evans");
        }

        [Fact]
        public async Task GetById_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            _mockRepo.Setup(r => r.SelectAsync(999)).ReturnsAsync((Artist?)null);

            // Act
            var result = await _controller.GetById(999);

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Search_ReturnsMatchingArtists()
        {
            // Arrange
            var artists = new List<Artist>
            {
                new() { ArtistId = 1, FirstName = "Miles", LastName = "Davis", Name = "Miles Davis" }
            };
            _mockRepo.Setup(r => r.GetArtistsByPartialNameAsync("Miles")).ReturnsAsync(artists);

            // Act
            var result = await _controller.Search("Miles");

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var dtos = okResult.Value.Should().BeAssignableTo<IEnumerable<ArtistDto>>().Subject.ToList();
            dtos.Should().ContainSingle();
            dtos[0].Name.Should().Be("Miles Davis");
        }

        [Fact]
        public async Task GetWithNoBiography_ReturnsArtistsWithoutBiography()
        {
            // Arrange
            var artists = new List<Artist>
            {
                new() { ArtistId = 10, FirstName = "Unknown", LastName = "Artist", Name = "Unknown Artist", Biography = "" }
            };
            _mockRepo.Setup(r => r.GetArtistsWithNoBiographyAsync()).ReturnsAsync(artists);

            // Act
            var result = await _controller.GetWithNoBiography();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var dtos = okResult.Value.Should().BeAssignableTo<IEnumerable<ArtistDto>>().Subject.ToList();
            dtos.Should().ContainSingle();
            dtos[0].ArtistId.Should().Be(10);
        }

        [Fact]
        public async Task GetWithNoBiographyByName_Existing_ReturnsOk()
        {
            // Arrange
            var artist = new Artist { ArtistId = 10, Name = "Test Artist", Biography = "" };
            _mockRepo.Setup(r => r.GetArtistWithNoBiographyAsync("Test Artist")).ReturnsAsync(artist);

            // Act
            var result = await _controller.GetWithNoBiographyByName("Test Artist");

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var dto = okResult.Value.Should().BeOfType<ArtistDto>().Subject;
            dto.ArtistId.Should().Be(10);
        }

        [Fact]
        public async Task GetWithNoBiographyByName_NotFound_ReturnsNotFound()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetArtistWithNoBiographyAsync("Missing")).ReturnsAsync((Artist?)null);

            // Act
            var result = await _controller.GetWithNoBiographyByName("Missing");

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetByRecord_ExistingRecord_ReturnsArtist()
        {
            // Arrange
            var artist = new Artist { ArtistId = 5, Name = "The Beatles" };
            _mockRepo.Setup(r => r.GetArtistByRecordIdAsync(100)).ReturnsAsync(artist);

            // Act
            var result = await _controller.GetByRecord(100);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var dto = okResult.Value.Should().BeOfType<ArtistDto>().Subject;
            dto.Name.Should().Be("The Beatles");
        }

        [Fact]
        public async Task GetByRecord_NotFound_ReturnsNotFound()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetArtistByRecordIdAsync(999)).ReturnsAsync((Artist?)null);

            // Act
            var result = await _controller.GetByRecord(999);

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetBiography_ReturnsBioText()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetBiographyAsync(1)).ReturnsAsync("Iconic jazz trumpeter.");

            // Act
            var result = await _controller.GetBiography(1);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().Be("Iconic jazz trumpeter.");
        }

        [Fact]
        public async Task GetArtistId_ReturnsId()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetArtistIdAsync("Miles", "Davis")).ReturnsAsync(1);

            // Act
            var result = await _controller.GetArtistId("Miles", "Davis");

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().Be(1);
        }

        [Fact]
        public async Task Create_Successful_ReturnsCreatedAtActionWithId()
        {
            // Arrange
            var createDto = new CreateArtistDto
            {
                FirstName = "Herbie",
                LastName = "Hancock",
                Biography = "Pianist & composer"
            };
            _mockRepo.Setup(r => r.InsertAsync(It.Is<Artist>(a => a.FirstName == "Herbie" && a.LastName == "Hancock")))
                     .ReturnsAsync(77);

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.ActionName.Should().Be(nameof(ArtistController.GetById));
            createdResult.RouteValues!["id"].Should().Be(77);
            createdResult.Value.Should().Be(77);
        }

        [Fact]
        public async Task Create_DuplicateOrFailed_ReturnsBadRequest()
        {
            // Arrange
            var createDto = new CreateArtistDto { FirstName = "Existing", LastName = "Artist" };
            _mockRepo.Setup(r => r.InsertAsync(It.IsAny<Artist>())).ReturnsAsync(0);

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var badRequest = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().Be("Artist could not be created. It may already exist.");
        }

        [Fact]
        public async Task Update_IdMismatch_ReturnsBadRequest()
        {
            // Arrange
            var updateDto = new UpdateArtistDto { ArtistId = 10, FirstName = "Test", Name = "Test" };

            // Act
            var result = await _controller.Update(5, updateDto);

            // Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().Be("Route id does not match body ArtistId.");
        }

        [Fact]
        public async Task Update_NotFound_ReturnsNotFound()
        {
            // Arrange
            var updateDto = new UpdateArtistDto { ArtistId = 99, FirstName = "Missing", Name = "Missing" };
            _mockRepo.Setup(r => r.SelectAsync(99)).ReturnsAsync((Artist?)null);

            // Act
            var result = await _controller.Update(99, updateDto);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Update_Existing_ReturnsNoContent()
        {
            // Arrange
            var existing = new Artist { ArtistId = 5, FirstName = "Old", Name = "Old" };
            var updateDto = new UpdateArtistDto { ArtistId = 5, FirstName = "New", Name = "New" };
            _mockRepo.Setup(r => r.SelectAsync(5)).ReturnsAsync(existing);
            _mockRepo.Setup(r => r.UpdateArtistAsync(It.IsAny<Artist>())).ReturnsAsync(1);

            // Act
            var result = await _controller.Update(5, updateDto);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            _mockRepo.Verify(r => r.UpdateArtistAsync(It.Is<Artist>(a => a.ArtistId == 5 && a.FirstName == "New")), Times.Once);
        }

        [Fact]
        public async Task Delete_NotFound_ReturnsNotFound()
        {
            // Arrange
            _mockRepo.Setup(r => r.SelectAsync(99)).ReturnsAsync((Artist?)null);

            // Act
            var result = await _controller.Delete(99);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Delete_Existing_ReturnsNoContent()
        {
            // Arrange
            var existing = new Artist { ArtistId = 5 };
            _mockRepo.Setup(r => r.SelectAsync(5)).ReturnsAsync(existing);
            _mockRepo.Setup(r => r.DeleteAsync(5)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Delete(5);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            _mockRepo.Verify(r => r.DeleteAsync(5), Times.Once);
        }
    }
}
