using Moq;
using Xunit;
using MSAuction.Application.Handlers;
using MSAuction.Application.Commands;
using MSAuction.Domain.Entities;
using MSAuction.Application.Interfaces;
using System.Threading.Tasks;
using System.Threading;
using MSAuction.Infraestructure.EventBus.Events;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;
using MSAuction.Application.DTOs;

namespace MsAuctionsTests.Application.HandlersTest
{
    public class UpdateAuctionHandlerTests
    {
        private Mock<IAuctionRepository> _repositoryMock;
        private readonly Mock<IAuctionEventPublisher> _eventPublisherMock = new();
        private readonly UpdateAuctionHandler _handler;
        public UpdateAuctionHandlerTests()
        {
            _repositoryMock = new Mock<IAuctionRepository>();
            _eventPublisherMock = new Mock<IAuctionEventPublisher>();

            _handler = new UpdateAuctionHandler(_repositoryMock.Object, _eventPublisherMock.Object);
        }
        [Fact]
        public async Task Handle_ShouldUpdateAuction_AndReturnTrue()
        {
            // Arrange
            var auctionId = 123;
            var userId = 1;

            var existingAuction = new Auction
            {
                Title = "Title",
                Id = auctionId,
                UserId = userId,
                Status = "pending"
            };

            var updateDto = new AuctionDto
            {
                Title = "Updated Title",
                InitialPrice = 100,
                MinIncrement = 10,
                ReservePrice = 150,
                Conditions = "Updated Conditions",
                Type = "simple",
                StartTime = DateTime.UtcNow.AddHours(1),
                EndTime = DateTime.UtcNow.AddDays(1)
            };

            var command = new UpdateAuctionCommand(auctionId, updateDto, userId);

            _repositoryMock.Setup(r => r.GetByIdAsync(auctionId)).ReturnsAsync(existingAuction);
            _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Auction>())).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result);
            _repositoryMock.Verify(r => r.UpdateAsync(It.Is<Auction>(a => a.Title == "Updated Title")), Times.Once);
            _eventPublisherMock.Verify(e => e.PublishAuctionUpdatedEvent(It.IsAny<AuctionUpdatedEvent>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldPublishEventToRabbitMQ()
        {
            // Arrange
            var auctionId = 123;
            var userId = 1;

            var auction = new Auction
            {
                Title = "Title",
                Id = auctionId,
                UserId = userId,
                Status = "draft"
            };

            var dto = new AuctionDto
            {
                Title = "Event Test",
                InitialPrice = 200,
                MinIncrement = 20,
                ReservePrice = 300,
                Conditions = "Event Conditions",
                Type = "live",
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(3)
            };

            var command = new UpdateAuctionCommand(auctionId, dto, userId);

            _repositoryMock.Setup(r => r.GetByIdAsync(auctionId)).ReturnsAsync(auction);
            _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Auction>())).Returns(Task.CompletedTask);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _eventPublisherMock.Verify(p => p.PublishAuctionUpdatedEvent(It.Is<AuctionUpdatedEvent>(
                e => e.AuctionId == auctionId &&
                     e.Title == "Event Test" &&
                     e.InitialPrice == 200 &&
                     e.Status == "draft"
            )), Times.Once);
        }
    }

}
