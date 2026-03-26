using AutoMapper;
using Contracts.Interfaces;
using MediatR;
using PartyManagement.Application.BankMaster.Command.Create;
using PartyManagement.Application.Common.Interfaces.IBankMaster;
using PartyManagement.Domain.Events;
using PartyManagement.UnitTests.TestData;
using Xunit;

namespace PartyManagement.UnitTests.Application.BankMaster.Commands
{
    public sealed class CreateBankMasterCommandHandlerTests
    {
        private readonly Mock<IBankMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IBankMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private CreateBankMasterHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockIp.Object, _mockMediator.Object);

        private void SetupHappyPath(int newId = 1)
        {
            var entity = BankMasterBuilders.ValidEntity(newId);

            _mockMapper
                .Setup(m => m.Map<PartyManagement.Domain.Entities.BankMaster>(It.IsAny<object>()))
                .Returns(entity);

            _mockQueryRepo
                .Setup(r => r.GenerateBankCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("BNK001");

            _mockCommandRepo
                .Setup(r => r.AddAsync(It.IsAny<PartyManagement.Domain.Entities.BankMaster>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(newId);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(5);
            var command = BankMasterBuilders.ValidCreateCommand();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(5);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsGreaterThanZero()
        {
            SetupHappyPath(1);
            var command = BankMasterBuilders.ValidCreateCommand();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsAddAsyncOnce()
        {
            SetupHappyPath();
            var command = BankMasterBuilders.ValidCreateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.AddAsync(It.IsAny<PartyManagement.Domain.Entities.BankMaster>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            var command = BankMasterBuilders.ValidCreateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_GeneratesBankCode()
        {
            SetupHappyPath();
            var command = BankMasterBuilders.ValidCreateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockQueryRepo.Verify(
                r => r.GenerateBankCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
