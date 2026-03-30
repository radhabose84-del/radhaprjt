using MediatR;
using PurchaseManagement.Application.Common.Interfaces.PriceMaster;
using PurchaseManagement.Application.PriceMaster.Commands.Delete;
using PurchaseManagement.Application.PriceMaster.Dtos;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.UnitTests.Application.PriceMaster.Commands
{
    public sealed class DeletePriceMasterCommandHandlerTests
    {
        private readonly Mock<IPriceMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IPriceMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeletePriceMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object);

        private void SetupHappyPath(int id = 1)
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PriceMasterGetAllDto { Id = id, ItemId = 1, ItemCode = "ITEM001" });

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(id))
                .ReturnsAsync(true);
        }

        [Fact]
        public async Task Handle_ExistingId_ReturnsTrue()
        {
            SetupHappyPath(1);

            var result = await CreateSut().Handle(
                new DeletePriceMasterCommand { Id = 1 },
                CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ExistingId_CallsDeleteOnce()
        {
            SetupHappyPath(1);

            await CreateSut().Handle(
                new DeletePriceMasterCommand { Id = 1 },
                CancellationToken.None);

            _mockCommandRepo.Verify(r => r.DeleteAsync(1), Times.Once);
        }

        [Fact]
        public async Task Handle_ExistingId_CallsQueryRepoOnce()
        {
            SetupHappyPath(1);

            await CreateSut().Handle(
                new DeletePriceMasterCommand { Id = 1 },
                CancellationToken.None);

            _mockQueryRepo.Verify(
                r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsException()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PriceMasterGetAllDto?)null);

            var sut = CreateSut();
            Func<Task> act = async () => await sut.Handle(
                new DeletePriceMasterCommand { Id = 99 },
                CancellationToken.None);

            await act.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task Handle_NotFound_DoesNotCallDelete()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PriceMasterGetAllDto?)null);

            var sut = CreateSut();
            try
            {
                await sut.Handle(new DeletePriceMasterCommand { Id = 99 }, CancellationToken.None);
            }
            catch { /* expected */ }

            _mockCommandRepo.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
        }
    }
}
