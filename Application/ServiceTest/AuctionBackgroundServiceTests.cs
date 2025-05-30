using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using MSAuction.Application.Interfaces;
using MSAuction.Application.Services;
using MSAuction.Domain.Entities;

namespace MsAuctionsTests.Application.ServiceTest
{
    public class AuctionBackgroundServiceTests
    {
        private readonly Mock<IAuctionRepository> _repositoryMock;
        private readonly AuctionBackgroundService _service;

        public AuctionBackgroundServiceTests()
        {
            _repositoryMock = new Mock<IAuctionRepository>();
            _service = new AuctionBackgroundService(_repositoryMock.Object);
        }

        [Fact]
        public async Task FinalizeAuction_ShouldDoNothing_WhenAuctionNotFound()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                           .ReturnsAsync((Auction)null);

            // Act
            await _service.FinalizeAuction(1);

            // Assert
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Auction>()), Times.Never);
        }

        [Fact]
        public async Task FinalizeAuction_ShouldDoNothing_WhenAuctionAlreadyFinalized()
        {
            // Arrange
            var auction = new Auction {Title= "Subasta", Id = 1, Status = "finalizada" };
            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(auction);

            // Act
            await _service.FinalizeAuction(1);

            // Assert
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Auction>()), Times.Never);
        }

        [Fact]
        public async Task FinalizeAuction_ShouldDoNothing_WhenAuctionNotEndedYet()
        {
            // Arrange
            var auction = new Auction
            {
                Title = "Subasta de prueba",
                Id = 1,
                Status = "activa",
                EndDate = DateTime.UtcNow.AddMinutes(5) // Aún no ha terminado
            };

            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(auction);

            // Act
            await _service.FinalizeAuction(1);

            // Assert
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Auction>()), Times.Never);
        }

        [Fact]
        public async Task FinalizeAuction_ShouldMarkAsEnded_AndUpdate_WhenAuctionIsOver()
        {
            // Arrange
            var auction = new Auction
            {
                Title = "Subasta de prueba",
                Id = 1,
                Status = "activa",
                EndDate = DateTime.UtcNow.AddMinutes(-1) // Ya terminó
            };

            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(auction);

            // Act
            await _service.FinalizeAuction(1);

            // Assert
            _repositoryMock.Verify(r => r.UpdateAsync(It.Is<Auction>(a => a.Status == "finalizada")), Times.Once);
        }
    }
}
