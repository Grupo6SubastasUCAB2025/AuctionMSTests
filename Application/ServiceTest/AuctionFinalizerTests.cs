using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MSAuction.Infraestructure.Database;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using Newtonsoft.Json;
using MSAuction.Infraestructure.EventBus.Events;
using MassTransit;
using Moq;
using MSAuction.Application.Interfaces;
using MSAuction.Application.Services;
using MSAuction.Domain.Entities;

namespace MsAuctionsTests.Application.ServiceTest
{
    public class AuctionFinalizerTests
    {
        private readonly Mock<IAuctionRepository> _repositoryMock;
        private readonly Mock<IPublishEndpoint> _publishEndpointMock;
        private readonly AuctionFinalizer _finalizer;

        public AuctionFinalizerTests()
        {
            _repositoryMock = new Mock<IAuctionRepository>();
            _publishEndpointMock = new Mock<IPublishEndpoint>();
            _finalizer = new AuctionFinalizer(_repositoryMock.Object, _publishEndpointMock.Object);
        }

        [Fact]
        public async Task FinalizeAuctionAsync_ShouldDoNothing_WhenAuctionNotFound()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                           .ReturnsAsync((Auction)null);

            // Act
            await _finalizer.FinalizeAuctionAsync(1);

            // Assert
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Auction>()), Times.Never);
            _publishEndpointMock.Verify(p => p.Publish(It.IsAny<AuctionEndedEvent>(), default), Times.Never);
        }

        [Fact]
        public async Task FinalizeAuctionAsync_ShouldDoNothing_WhenAuctionIsNotPending()
        {
            // Arrange
            var auction = new Auction {Title="subasta", Id = 1, Status = "finalizada" };

            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(auction);

            // Act
            await _finalizer.FinalizeAuctionAsync(1);

            // Assert
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Auction>()), Times.Never);
            _publishEndpointMock.Verify(p => p.Publish(It.IsAny<AuctionEndedEvent>(), default), Times.Never);
        }

        [Fact]
        public async Task FinalizeAuctionAsync_ShouldFinalizeAndPublish_WhenAuctionIsPending()
        {
            // Arrange
            var auction = new Auction
            {
                Title = "Subasta de prueba",
                Id = 1,
                Status = "pending",
                EndDate = DateTime.UtcNow.AddMinutes(-10)
            };

            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(auction);

            // Act
            await _finalizer.FinalizeAuctionAsync(1);

            // Assert
            _repositoryMock.Verify(r => r.UpdateAsync(It.Is<Auction>(a => a.Status == "finalizada")), Times.Once);
            _publishEndpointMock.Verify(p => p.Publish(It.Is<AuctionEndedEvent>(e => e.AuctionId == 1), default), Times.Once);
        }
    }
}
