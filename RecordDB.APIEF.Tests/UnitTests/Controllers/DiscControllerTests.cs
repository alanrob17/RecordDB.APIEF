using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RecordDB.APIEF.Controllers;
using RecordDB.APIEF.DTOs;
using RecordDB.APIEF.Models;
using RecordDB.APIEF.Repositories;

namespace RecordDB.APIEF.Tests.UnitTests.Controllers
{
    public class DiscControllerTests
    {
        private readonly Mock<IDiscRepository> _mockRepo;
        private readonly DiscController _controller;

        public DiscControllerTests()
        {
            _mockRepo = new Mock<IDiscRepository>();
            _controller = new DiscController(_mockRepo.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOkWithDiscs()
        {
            // Arrange
            var discs = new List<ArtistRecordDiscDto>
            {
                new() { DiscId = 1, RecordId = 10, DiscNo = 1, Name = "Album 1" },
                new() { DiscId = 2, RecordId = 10, DiscNo = 2, Name = "Album 1" }
            };
            _mockRepo.Setup(r => r.SelectAllDiscEntitiesAsync()).ReturnsAsync(discs);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var dtos = okResult.Value.Should().BeAssignableTo<IEnumerable<ArtistRecordDiscDto>>().Subject.ToList();
            dtos.Should().HaveCount(2);
            dtos[0].DiscId.Should().Be(1);
        }

        [Fact]
        public async Task GetById_Existing_ReturnsOk()
        {
            // Arrange
            var disc = new ArtistRecordDiscDto { DiscId = 10, RecordId = 5, DiscNo = 1 };
            _mockRepo.Setup(r => r.SelectSingleDiscAsync(10)).ReturnsAsync(disc);

            // Act
            var result = await _controller.GetById(10);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var dto = okResult.Value.Should().BeOfType<ArtistRecordDiscDto>().Subject;
            dto.DiscId.Should().Be(10);
        }

        [Fact]
        public async Task GetById_NotFound_ReturnsNotFound()
        {
            // Arrange
            _mockRepo.Setup(r => r.SelectSingleDiscAsync(99)).ReturnsAsync((ArtistRecordDiscDto?)null);

            // Act
            var result = await _controller.GetById(99);

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task SearchByRecordName_ReturnsMatchingDiscs()
        {
            // Arrange
            var discs = new List<ArtistRecordDiscDto>
            {
                new() { DiscId = 1, Name = "Kind of Blue" }
            };
            _mockRepo.Setup(r => r.GetDiscRecordsByRecordNameAsync("Blue")).ReturnsAsync(discs);

            // Act
            var result = await _controller.SearchByRecordName("Blue");

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var dtos = okResult.Value.Should().BeAssignableTo<IEnumerable<ArtistRecordDiscDto>>().Subject.ToList();
            dtos.Should().ContainSingle();
            dtos[0].Name.Should().Be("Kind of Blue");
        }

        [Fact]
        public async Task Create_Successful_ReturnsCreatedAtActionWithId()
        {
            // Arrange
            var createDto = new CreateDiscDto
            {
                RecordId = 5,
                DiscNo = 1,
                Length = 2400
            };
            _mockRepo.Setup(r => r.InsertDiscAsync(It.Is<Disc>(d => d.RecordId == 5 && d.DiscNo == 1)))
                     .ReturnsAsync(15);

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.ActionName.Should().Be(nameof(DiscController.GetById));
            createdResult.RouteValues!["id"].Should().Be(15);
            createdResult.Value.Should().Be(15);
        }

        [Fact]
        public async Task Create_Failed_ReturnsBadRequest()
        {
            // Arrange
            var createDto = new CreateDiscDto { RecordId = 5, DiscNo = 1 };
            _mockRepo.Setup(r => r.InsertDiscAsync(It.IsAny<Disc>())).ReturnsAsync(0);

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var badRequest = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().Be("Disc could not be created.");
        }

        [Fact]
        public async Task Update_IdMismatch_ReturnsBadRequest()
        {
            // Arrange
            var updateDto = new UpdateDiscDto { DiscId = 10, DiscNo = 1 };

            // Act
            var result = await _controller.Update(5, updateDto);

            // Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().Be("Route id does not match body DiscId.");
        }

        [Fact]
        public async Task Update_NotFound_ReturnsNotFound()
        {
            // Arrange
            var updateDto = new UpdateDiscDto { DiscId = 99, DiscNo = 1 };
            _mockRepo.Setup(r => r.SelectSingleDiscAsync(99)).ReturnsAsync((ArtistRecordDiscDto?)null);

            // Act
            var result = await _controller.Update(99, updateDto);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Update_Existing_ReturnsNoContent()
        {
            // Arrange
            var existing = new ArtistRecordDiscDto { DiscId = 10, DiscNo = 1 };
            var updateDto = new UpdateDiscDto { DiscId = 10, DiscNo = 2 };
            _mockRepo.Setup(r => r.SelectSingleDiscAsync(10)).ReturnsAsync(existing);
            _mockRepo.Setup(r => r.UpdateDiscAsync(It.IsAny<Disc>())).ReturnsAsync(1);

            // Act
            var result = await _controller.Update(10, updateDto);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            _mockRepo.Verify(r => r.UpdateDiscAsync(It.Is<Disc>(d => d.DiscId == 10 && d.DiscNo == 2)), Times.Once);
        }

        [Fact]
        public async Task UpdateLength_NotFound_ReturnsNotFound()
        {
            // Arrange
            _mockRepo.Setup(r => r.SelectSingleDiscAsync(99)).ReturnsAsync((ArtistRecordDiscDto?)null);

            // Act
            var result = await _controller.UpdateLength(99, new UpdateDiscLengthDto { Length = 3000 });

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task UpdateLength_Existing_ReturnsNoContent()
        {
            // Arrange
            var existing = new ArtistRecordDiscDto { DiscId = 10 };
            _mockRepo.Setup(r => r.SelectSingleDiscAsync(10)).ReturnsAsync(existing);
            _mockRepo.Setup(r => r.UpdateDiscLengthAsync(10, 3000)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UpdateLength(10, new UpdateDiscLengthDto { Length = 3000 });

            // Assert
            result.Should().BeOfType<NoContentResult>();
            _mockRepo.Verify(r => r.UpdateDiscLengthAsync(10, 3000), Times.Once);
        }

        [Fact]
        public async Task Delete_NotFound_ReturnsNotFound()
        {
            // Arrange
            _mockRepo.Setup(r => r.SelectSingleDiscAsync(99)).ReturnsAsync((ArtistRecordDiscDto?)null);

            // Act
            var result = await _controller.Delete(99);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Delete_Existing_ReturnsNoContent()
        {
            // Arrange
            var existing = new ArtistRecordDiscDto { DiscId = 10 };
            _mockRepo.Setup(r => r.SelectSingleDiscAsync(10)).ReturnsAsync(existing);
            _mockRepo.Setup(r => r.DeleteDiscAsync(10)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Delete(10);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            _mockRepo.Verify(r => r.DeleteDiscAsync(10), Times.Once);
        }
    }
}
