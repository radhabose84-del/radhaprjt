using Contracts.Interfaces;
using MediatR;
using PartyManagement.Application.BankAccount.Command.UpdateBankAccount;
using PartyManagement.Application.Common.Interfaces.IBankAccount;
using PartyManagement.Application.Party.BankAccounts.Commands.UpdateBankAccount;
using PartyManagement.Domain.Common;
using PartyManagement.Domain.Events;
using Xunit;

namespace PartyManagement.UnitTests.Application.BankAccount.Commands
{
    public sealed class UpdateBankAccountCommandHandlerTests
    {
        private readonly Mock<IBankAccountCommandRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private UpdateBankAccountCommandHandler CreateSut() =>
            new(_mockRepo.Object, _mockIp.Object, _mockMediator.Object);

        private static UpdateBankAccountCommand ValidCommand(int id = 1) =>
            new UpdateBankAccountCommand(
                Id: id,
                BankId: 1,
                AccountNumber: "1234567890",
                AccountHolderName: "Test Holder",
                BranchId: 1,
                IFSCCode: "ICIC0001234",
                SWIFTCode: null,
                AccountTypeId: 1,
                IsDefaultAccount: true,
                IsPrimaryAccount: true,
                IBan: null,
                IsActive: BaseEntity.Status.Active
            );

        [Fact]
        public async Task Handle_ExistingId_ReturnsTrue()
        {
            var entity = new PartyManagement.Domain.Entities.BankAccount
            {
                Id = 1, BankId = 1, AccountNumber = "1234567890", AccountHolderName = "Test"
            };

            _mockRepo.Setup(r => r.FindAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
            _mockRepo.Setup(r => r.UpdateAsync(entity, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(ValidCommand(1), CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_NotFound_ReturnsFalse()
        {
            _mockRepo
                .Setup(r => r.FindAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PartyManagement.Domain.Entities.BankAccount?)null);

            var result = await CreateSut().Handle(ValidCommand(99), CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_ExistingId_CallsUpdateOnce()
        {
            var entity = new PartyManagement.Domain.Entities.BankAccount
            {
                Id = 1, BankId = 1, AccountNumber = "1234567890", AccountHolderName = "Test"
            };

            _mockRepo.Setup(r => r.FindAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
            _mockRepo.Setup(r => r.UpdateAsync(entity, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            await CreateSut().Handle(ValidCommand(1), CancellationToken.None);

            _mockRepo.Verify(
                r => r.UpdateAsync(entity, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ExistingId_PublishesAuditEvent()
        {
            var entity = new PartyManagement.Domain.Entities.BankAccount
            {
                Id = 1, BankId = 1, AccountNumber = "1234567890", AccountHolderName = "Test"
            };

            _mockRepo.Setup(r => r.FindAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
            _mockRepo.Setup(r => r.UpdateAsync(entity, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            await CreateSut().Handle(ValidCommand(1), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
