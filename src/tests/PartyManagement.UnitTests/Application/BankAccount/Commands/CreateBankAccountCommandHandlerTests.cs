using MediatR;
using PartyManagement.Application.BankAccount.Command.CreateBankAccount;
using PartyManagement.Application.Common.Interfaces.IBankAccount;
using PartyManagement.Domain.Events;
using Xunit;

namespace PartyManagement.UnitTests.Application.BankAccount.Commands
{
    public sealed class CreateBankAccountCommandHandlerTests
    {
        private readonly Mock<IBankAccountCommandRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private CreateBankAccountCommandHandler CreateSut() =>
            new(_mockRepo.Object, _mockMediator.Object);

        private static CreateBankAccountCommand ValidCommand() =>
            new CreateBankAccountCommand(
                BankId: 1,
                AccountNumber: "1234567890",
                AccountHolderName: "Test Holder",
                BranchId: 1,
                IFSCCode: "ICIC0001234",
                SWIFTCode: null,
                AccountTypeId: 1,
                IsDefaultAccount: true,
                IsPrimaryAccount: true,
                IBan: null
            );

        private void SetupHappyPath(int newId = 1)
        {
            var entity = new PartyManagement.Domain.Entities.BankAccount
            {
                Id = newId,
                BankId = 1,
                AccountNumber = "1234567890",
                AccountHolderName = "Test Holder"
            };

            _mockRepo
                .Setup(r => r.AddAsync(It.IsAny<PartyManagement.Domain.Entities.BankAccount>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(5);

            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.Should().Be(5);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsGreaterThanZero()
        {
            SetupHappyPath(1);

            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsAddOnce()
        {
            SetupHappyPath();

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockRepo.Verify(
                r => r.AddAsync(It.IsAny<PartyManagement.Domain.Entities.BankAccount>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
