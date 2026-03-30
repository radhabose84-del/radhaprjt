using Contracts.Interfaces;
using PartyManagement.Application.BankMaster.Command.Delete;
using PartyManagement.Application.Common.Interfaces.IBankMaster;
using PartyManagement.UnitTests.TestData;
using Xunit;

namespace PartyManagement.UnitTests.Application.BankMaster.Commands
{
    public sealed class DeleteBankMasterCommandHandlerTests
    {
        private readonly Mock<IBankMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IBankMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);

        private DeleteBankMasterCommandHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockCommandRepo.Object, _mockIp.Object);

        [Fact]
        public async Task Handle_ValidId_CompletesWithoutException()
        {
            var entity = BankMasterBuilders.ValidEntity(1);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);

            _mockCommandRepo
                .Setup(r => r.SoftDeleteAsync(entity, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            Func<Task> act = async () => await CreateSut().Handle(new DeleteBankMasterCommand(1), CancellationToken.None);

            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task Handle_ValidId_CallsSoftDeleteOnce()
        {
            var entity = BankMasterBuilders.ValidEntity(1);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);

            _mockCommandRepo
                .Setup(r => r.SoftDeleteAsync(entity, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(new DeleteBankMasterCommand(1), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.SoftDeleteAsync(entity, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsKeyNotFoundException()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PartyManagement.Domain.Entities.BankMaster?)null);

            Func<Task> act = async () => await CreateSut().Handle(new DeleteBankMasterCommand(99), CancellationToken.None);

            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_NotFound_DoesNotCallSoftDelete()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PartyManagement.Domain.Entities.BankMaster?)null);

            try
            {
                await CreateSut().Handle(new DeleteBankMasterCommand(99), CancellationToken.None);
            }
            catch { /* expected */ }

            _mockCommandRepo.Verify(
                r => r.SoftDeleteAsync(It.IsAny<PartyManagement.Domain.Entities.BankMaster>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
