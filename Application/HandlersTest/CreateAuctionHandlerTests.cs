using Xunit;
using Moq;
using Hangfire;
using Hangfire.MemoryStorage;
using System.Threading;
using System.Threading.Tasks;
using MSAuction.Application.Handlers;
using MSAuction.Application.Commands;
using MSAuction.Application.DTOs;
using MSAuction.Domain.Entities;
using MSAuction.Application.Interfaces;
using FluentAssertions;

namespace MsAuctionsTests.Application.HandlersTest
{
    public class CreateAuctionHandlerTests
    {
        public CreateAuctionHandlerTests()
        {
            // Configuración para usar Hangfire en memoria
            GlobalConfiguration.Configuration.UseMemoryStorage();
        }

        [Fact]
        public async Task Handle_ShouldAddAuction_AndReturnId()
        {
            // Arrange
            var mockRepository = new Mock<IAuctionRepository>();

            // Simulamos que el ID se asigna automáticamente después de la inserción
            mockRepository.Setup(r => r.AddAsync(It.IsAny<Auction>()))
                .Callback<Auction>(auction =>
                {
                    // Asignamos un ID simulado como si fuera autoincremental
                    auction.Id = 42;
                });

            var handler = new CreateAuctionHandler(mockRepository.Object);
            var auctionDto = new AuctionDto
            {
                ProductId = 1,
                Title = "Test Auction",
                Description = "Description",
                InitialPrice = 100,
                MinIncrement = 10,
                ReservePrice = 200,
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddDays(1),
                Conditions = "New"
            };
            var command = new CreateAuctionCommand(auctionDto, userId: 123);


            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(42, result);  // Verificamos que el ID asignado sea el esperado
        }
    }
}
