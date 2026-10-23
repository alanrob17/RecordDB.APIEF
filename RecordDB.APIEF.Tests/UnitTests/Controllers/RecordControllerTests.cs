using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RecordDB.APIEF.Controllers;
using RecordDB.APIEF.DTOs;
using RecordDB.APIEF.Models;
using RecordDB.APIEF.Repositories;
using RecordModel = RecordDB.APIEF.Models.Record;

namespace RecordDB.APIEF.Tests.UnitTests.Controllers
{
    public class RecordControllerTests
    {
        private readonly Mock<IRecordRepository> _mockRepo;
        private readonly RecordController _controller;

        public RecordControllerTests()
        {
            _mockRepo = new Mock<IRecordRepository>();
            _controller = new RecordController(_mockRepo.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOkWithRecords()
        {
            // Arrange
            var records = new List<ArtistRecordDto>
            {
                new() { RecordId = 1, Name = "Kind of Blue", ArtistName = "Miles Davis" },
                new() { RecordId = 2, Name = "A Love Supreme", ArtistName = "John Coltrane" }
            };
            _mockRepo.Setup(r => r.SelectAsync()).ReturnsAsync(records);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var dtos = okResult.Value.Should().BeAssignableTo<IEnumerable<ArtistRecordDto>>().Subject.ToList();
            dtos.Should().HaveCount(2);
            dtos[0].Name.Should().Be("Kind of Blue");
        }

        [Fact]
        public async Task GetByShow_ReturnsFilteredRecords()
        {
            // Arrange
            var records = new List<ArtistRecordDto>
            {
                new() { RecordId = 1, Name = "Kind of Blue" }
            };
            _mockRepo.Setup(r => r.SelectRecordsShowAsync("Y")).ReturnsAsync(records);

            // Act
            var result = await _controller.GetByShow("Y");

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var dtos = okResult.Value.Should().BeAssignableTo<IEnumerable<ArtistRecordDto>>().Subject.ToList();
            dtos.Should().ContainSingle();
        }

        [Fact]
        public async Task GetById_Existing_ReturnsOk()
        {
            // Arrange
            var record = new ArtistRecordDto { RecordId = 10, Name = "Giant Steps" };
            _mockRepo.Setup(r => r.SelectAsync(10)).ReturnsAsync(record);

            // Act
            var result = await _controller.GetById(10);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var dto = okResult.Value.Should().BeOfType<ArtistRecordDto>().Subject;
            dto.RecordId.Should().Be(10);
            dto.Name.Should().Be("Giant Steps");
        }

        [Fact]
        public async Task GetById_NotFound_ReturnsNotFound()
        {
            // Arrange
            _mockRepo.Setup(r => r.SelectAsync(99)).ReturnsAsync((ArtistRecordDto?)null);

            // Act
            var result = await _controller.GetById(99);

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetByArtistName_ReturnsRecords()
        {
            // Arrange
            var records = new List<ArtistRecordDto>
            {
                new() { RecordId = 1, Name = "Kind of Blue", ArtistName = "Miles Davis" }
            };
            _mockRepo.Setup(r => r.GetRecordsByArtistNameAsync("Miles Davis")).ReturnsAsync(records);

            // Act
            var result = await _controller.GetByArtistName("Miles Davis");

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var dtos = okResult.Value.Should().BeAssignableTo<IEnumerable<ArtistRecordDto>>().Subject.ToList();
            dtos.Should().ContainSingle();
        }

        [Fact]
        public async Task GetByYear_ReturnsRecords()
        {
            // Arrange
            var records = new List<ArtistRecordDto>
            {
                new() { RecordId = 1, Name = "Kind of Blue", Recorded = 1959 }
            };
            _mockRepo.Setup(r => r.GetRecordsByYearAsync(1959)).ReturnsAsync(records);

            // Act
            var result = await _controller.GetByYear(1959);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var dtos = okResult.Value.Should().BeAssignableTo<IEnumerable<ArtistRecordDto>>().Subject.ToList();
            dtos.Should().ContainSingle();
        }

        [Fact]
        public async Task GetReviews_ReturnsReviews()
        {
            // Arrange
            var reviews = new List<RecordReviewDto>
            {
                new() { Name = "Miles Davis", Title = "Kind of Blue", Review = "Masterpiece" }
            };
            _mockRepo.Setup(r => r.SelectRecordReviewsAsync()).ReturnsAsync(reviews);

            // Act
            var result = await _controller.GetReviews();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var dtos = okResult.Value.Should().BeAssignableTo<IEnumerable<RecordReviewDto>>().Subject.ToList();
            dtos.Should().ContainSingle();
            dtos[0].Review.Should().Be("Masterpiece");
        }

        [Fact]
        public async Task GetMissingReviews_ReturnsMissing()
        {
            // Arrange
            var missing = new List<MissingReviewDto>
            {
                new() { RecordId = 5, Name = "In a Silent Way" }
            };
            _mockRepo.Setup(r => r.NoRecordReviewsAsync()).ReturnsAsync(missing);

            // Act
            var result = await _controller.GetMissingReviews();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var dtos = okResult.Value.Should().BeAssignableTo<IEnumerable<MissingReviewDto>>().Subject.ToList();
            dtos.Should().ContainSingle();
        }

        [Fact]
        public async Task GetWithNoTracks_ReturnsDiscs()
        {
            // Arrange
            var discs = new List<ArtistRecordDiscDto>
            {
                new() { DiscId = 1, RecordId = 5, Name = "Album" }
            };
            _mockRepo.Setup(r => r.ListRecordsWithNoTracksAsync()).ReturnsAsync(discs);

            // Act
            var result = await _controller.GetWithNoTracks();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var dtos = okResult.Value.Should().BeAssignableTo<IEnumerable<ArtistRecordDiscDto>>().Subject.ToList();
            dtos.Should().ContainSingle();
        }

        [Fact]
        public async Task GetTotals_ReturnsTotals()
        {
            // Arrange
            var totals = new List<Total>
            {
                new() { ArtistId = 1, Name = "Miles Davis", TotalDiscs = 10, TotalCost = 150.0m }
            };
            _mockRepo.Setup(r => r.GetTotalCostsAsync()).ReturnsAsync(totals);

            // Act
            var result = await _controller.GetTotals();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var dtos = okResult.Value.Should().BeAssignableTo<IEnumerable<Total>>().Subject.ToList();
            dtos.Should().ContainSingle();
            dtos[0].Name.Should().Be("Miles Davis");
        }

        [Fact]
        public async Task Create_Successful_ReturnsCreatedAtActionWithId()
        {
            // Arrange
            var createDto = new CreateRecordDto
            {
                ArtistId = 1,
                Name = "Milestones",
                Discs = 1
            };
            _mockRepo.Setup(r => r.InsertAsync(It.Is<RecordModel>(rec => rec.Name == "Milestones")))
                     .ReturnsAsync(88);

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.ActionName.Should().Be(nameof(RecordController.GetById));
            createdResult.RouteValues!["id"].Should().Be(88);
            createdResult.Value.Should().Be(88);
        }

        [Fact]
        public async Task Create_Failed_ReturnsBadRequest()
        {
            // Arrange
            var createDto = new CreateRecordDto { ArtistId = 1, Name = "Failed" };
            _mockRepo.Setup(r => r.InsertAsync(It.IsAny<RecordModel>())).ReturnsAsync(0);

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var badRequest = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().Be("Record could not be created. It may already exist.");
        }

        [Fact]
        public async Task Update_IdMismatch_ReturnsBadRequest()
        {
            // Arrange
            var updateDto = new UpdateRecordDto { RecordId = 10, Name = "Name" };

            // Act
            var result = await _controller.Update(5, updateDto);

            // Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().Be("Route id does not match body RecordId.");
        }

        [Fact]
        public async Task Update_NotFound_ReturnsNotFound()
        {
            // Arrange
            var updateDto = new UpdateRecordDto { RecordId = 99, Name = "Missing" };
            _mockRepo.Setup(r => r.SelectAsync(99)).ReturnsAsync((ArtistRecordDto?)null);

            // Act
            var result = await _controller.Update(99, updateDto);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Update_Existing_ReturnsNoContent()
        {
            // Arrange
            var existing = new ArtistRecordDto { RecordId = 10, Name = "Old" };
            var updateDto = new UpdateRecordDto { RecordId = 10, Name = "Updated" };
            _mockRepo.Setup(r => r.SelectAsync(10)).ReturnsAsync(existing);
            _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<ArtistRecordDto>())).ReturnsAsync(1);

            // Act
            var result = await _controller.Update(10, updateDto);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            _mockRepo.Verify(r => r.UpdateAsync(It.Is<ArtistRecordDto>(dto => dto.RecordId == 10 && dto.Name == "Updated")), Times.Once);
        }

        [Fact]
        public async Task Delete_NotFound_ReturnsNotFound()
        {
            // Arrange
            _mockRepo.Setup(r => r.SelectAsync(99)).ReturnsAsync((ArtistRecordDto?)null);

            // Act
            var result = await _controller.Delete(99);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Delete_Existing_ReturnsNoContent()
        {
            // Arrange
            var existing = new ArtistRecordDto { RecordId = 10 };
            _mockRepo.Setup(r => r.SelectAsync(10)).ReturnsAsync(existing);
            _mockRepo.Setup(r => r.DeleteAsync(10)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Delete(10);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            _mockRepo.Verify(r => r.DeleteAsync(10), Times.Once);
        }
    }
}
