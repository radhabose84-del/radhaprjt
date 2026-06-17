using Contracts.Commands.Finance;
using FinanceManagement.Application.Common.Interfaces.IAccountGroup;
using FinanceManagement.Application.Consumers;
using FinanceManagement.Domain.Common;
using FinanceManagement.Domain.Entities;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FinanceManagement.UnitTests.Application.Consumers
{
    public sealed class ApprovedRejectedConsumerTests
    {
        private readonly Mock<IAccountGroupChangeRequestRepository> _mockChangeRequestRepo = new(MockBehavior.Loose);
        private readonly Mock<IAccountGroupCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);

        private ApprovedRejectedConsumer CreateSut() =>
            new(_mockChangeRequestRepo.Object, _mockCommandRepo.Object,
                new Mock<ILogger<ApprovedRejectedConsumer>>().Object);

        private static ConsumeContext<UpdateApprovedRejectedFinanceCommand> Ctx(string moduleTypeName, string status, int moduleTransactionId = 3)
        {
            var ctx = new Mock<ConsumeContext<UpdateApprovedRejectedFinanceCommand>>();
            ctx.SetupGet(c => c.Message).Returns(new UpdateApprovedRejectedFinanceCommand
            {
                CorrelationId = Guid.NewGuid(),
                ModuleTypeName = moduleTypeName,
                ModuleTransactionId = moduleTransactionId,
                Status = status
            });
            ctx.SetupGet(c => c.CancellationToken).Returns(CancellationToken.None);
            return ctx.Object;
        }

        private void SetupPendingChangeRequest()
        {
            _mockChangeRequestRepo
                .Setup(r => r.GetPendingByAccountGroupAsync(3, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AccountGroupChangeRequest { Id = 1, AccountGroupId = 3, NewParentAccountGroupId = 5 });
        }

        [Fact]
        public async Task Consume_Approved_AppliesMoveAndMarksApproved()
        {
            SetupPendingChangeRequest();

            await CreateSut().Consume(Ctx(MiscEnumEntity.AccountGroupHierarchy, MiscEnumEntity.Approved));

            _mockCommandRepo.Verify(r => r.MoveAsync(3, 5), Times.Once);
            _mockChangeRequestRepo.Verify(r => r.MarkStatusAsync(1, MiscEnumEntity.Approved, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Consume_Rejected_MarksRejectedAndDoesNotMove()
        {
            SetupPendingChangeRequest();

            await CreateSut().Consume(Ctx(MiscEnumEntity.AccountGroupHierarchy, MiscEnumEntity.Rejected));

            _mockChangeRequestRepo.Verify(r => r.MarkStatusAsync(1, MiscEnumEntity.Rejected, It.IsAny<CancellationToken>()), Times.Once);
            _mockCommandRepo.Verify(r => r.MoveAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task Consume_OtherModuleType_Ignored()
        {
            await CreateSut().Consume(Ctx("Purchase Order", MiscEnumEntity.Approved));

            _mockChangeRequestRepo.Verify(r => r.GetPendingByAccountGroupAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockCommandRepo.Verify(r => r.MoveAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task Consume_NoPendingChangeRequest_DoesNothing()
        {
            _mockChangeRequestRepo
                .Setup(r => r.GetPendingByAccountGroupAsync(3, It.IsAny<CancellationToken>()))
                .ReturnsAsync((AccountGroupChangeRequest?)null);

            await CreateSut().Consume(Ctx(MiscEnumEntity.AccountGroupHierarchy, MiscEnumEntity.Approved));

            _mockCommandRepo.Verify(r => r.MoveAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            _mockChangeRequestRepo.Verify(r => r.MarkStatusAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Consume_UnknownStatus_Ignored()
        {
            await CreateSut().Consume(Ctx(MiscEnumEntity.AccountGroupHierarchy, "Pending"));

            _mockChangeRequestRepo.Verify(r => r.GetPendingByAccountGroupAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockCommandRepo.Verify(r => r.MoveAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }
    }
}
