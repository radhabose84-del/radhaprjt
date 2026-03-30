using Contracts.Common;
using MediatR;
using PartyManagement.Application.BankAccount.Command.DeleteBankAccount;
using PartyManagement.Application.Common.Interfaces.IBankAccount;
using PartyManagement.Domain.Events;
using Xunit;

namespace PartyManagement.UnitTests.Application.BankAccount.Commands
{
    public sealed class DeleteBankAccountCommandHandlerTests
    {
        private readonly Mock<IBankAccountCommandRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteBankAccountCommandHandler CreateSut() =>
            new(_mockRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsTrue()
        {
            var entity = new PartyManagement.Domain.Entities.BankAccount
            {
                Id = 1, BankId = 1, AccountNumber = "1234567890"
            };

            _mockRepo.Setup(r => r.FindAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
            _mockRepo.Setup(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await CreateSut().Handle(new DeleteBankAccountCommand(1), CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsExceptionRules()
        {
            _mockRepo
                .Setup(r => r.FindAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PartyManagement.Domain.Entities.BankAccount?)null);

            Func<Task> act = async () => await CreateSut().Handle(new DeleteBankAccountCommand(99), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_ExistingId_PublishesAuditEvent()
        {
            var entity = new PartyManagement.Domain.Entities.BankAccount
            {
                Id = 1, BankId = 1, AccountNumber = "1234567890"
            };

            _mockRepo.Setup(r => r.FindAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
            _mockRepo.Setup(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            await CreateSut().Handle(new DeleteBankAccountCommand(1), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ExistingId_CallsSoftDeleteOnce()
        {
            var entity = new PartyManagement.Domain.Entities.BankAccount
            {
                Id = 1, BankId = 1, AccountNumber = "1234567890"
            };

            _mockRepo.Setup(r => r.FindAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
            _mockRepo.Setup(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            await CreateSut().Handle(new DeleteBankAccountCommand(1), CancellationToken.None);

            _mockRepo.Verify(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
