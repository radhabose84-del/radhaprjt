using AutoMapper;
using Contracts.Interfaces;
using PartyManagement.Application.BankMaster;
using PartyManagement.Application.BankMaster.Command.Update;
using PartyManagement.Application.Common.Interfaces.IBankMaster;
using PartyManagement.UnitTests.TestData;
using Xunit;

namespace PartyManagement.UnitTests.Application.BankMaster.Commands
{
    public sealed class UpdateBankMasterCommandHandlerTests
    {
        private readonly Mock<IBankMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IBankMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);

        private UpdateBankMasterCommandHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockCommandRepo.Object, _mockMapper.Object, _mockIp.Object);

        private void SetupHappyPath(int id = 1)
        {
            var entity = BankMasterBuilders.ValidEntity(id);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);

            _mockQueryRepo
                .Setup(r => r.ExistsByBankCodeAsync(It.IsAny<string>(), id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(entity, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_CompletesWithoutException()
        {
            SetupHappyPath(1);
            var command = new UpdateBankMasterCommand(new UpdateBankMasterDto(1, "ICICI Bank Updated", 1));

            var act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath(1);
            var command = new UpdateBankMasterCommand(new UpdateBankMasterDto(1, "ICICI Bank Updated", 1));

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<PartyManagement.Domain.Entities.BankMaster>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsKeyNotFoundException()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PartyManagement.Domain.Entities.BankMaster?)null);

            var command = new UpdateBankMasterCommand(new UpdateBankMasterDto(99, "Test Bank", 1));

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_DuplicateName_ThrowsInvalidOperationException()
        {
            var entity = BankMasterBuilders.ValidEntity(1);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);

            _mockQueryRepo
                .Setup(r => r.ExistsByBankCodeAsync(It.IsAny<string>(), 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var command = new UpdateBankMasterCommand(new UpdateBankMasterDto(1, "Duplicate Bank", 1));

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*already exists*");
        }
    }
}
