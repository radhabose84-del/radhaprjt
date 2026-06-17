using Contracts.Commands.Finance;
using FinanceManagement.Application.Common.Interfaces.IAccountGroup;
using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Application.Consumers;
using FinanceManagement.Domain.Common;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FinanceManagement.UnitTests.Application.Consumers
{
    public sealed class ApprovedRejectedConsumerTests
    {
        private readonly Mock<IAccountGroupChangeRequestRepository> _changeRepo = new(MockBehavior.Loose);
        private readonly Mock<IAccountGroupCommandRepository> _agCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<ITaxCodeCommandRepository> _taxCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<ITaxCodeQueryRepository> _taxQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<ILogger<ApprovedRejectedConsumer>> _logger = new();

        private const int ApprovedStatusId = 48;
        private const int RejectedStatusId = 50;
        private const int LinkageId = 21;

        private ApprovedRejectedConsumer CreateSut() =>
            new(_changeRepo.Object, _agCommandRepo.Object, _taxCommandRepo.Object, _taxQueryRepo.Object, _logger.Object);

        private static Mock<ConsumeContext<UpdateApprovedRejectedFinanceCommand>> Context(string status) =>
            BuildContext(new UpdateApprovedRejectedFinanceCommand
            {
                CorrelationId = Guid.NewGuid(),
                ModuleTransactionId = LinkageId,
                ModuleTypeName = MiscEnumEntity.TaxAccountLinkage,
                Status = status
            });

        private static Mock<ConsumeContext<UpdateApprovedRejectedFinanceCommand>> BuildContext(UpdateApprovedRejectedFinanceCommand msg)
        {
            var ctx = new Mock<ConsumeContext<UpdateApprovedRejectedFinanceCommand>>(MockBehavior.Loose);
            ctx.SetupGet(c => c.Message).Returns(msg);
            ctx.SetupGet(c => c.CancellationToken).Returns(CancellationToken.None);
            return ctx;
        }

        [Fact]
        public async Task Consume_TaxLinkageApproved_ActivatesLinkage()
        {
            _taxQueryRepo.Setup(r => r.GetMiscIdAsync(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Approved)).ReturnsAsync(ApprovedStatusId);
            _taxCommandRepo.Setup(r => r.ActivateLinkageAsync(LinkageId, ApprovedStatusId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            await CreateSut().Consume(Context(MiscEnumEntity.Approved).Object);

            _taxCommandRepo.Verify(r => r.ActivateLinkageAsync(LinkageId, ApprovedStatusId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Consume_TaxLinkageRejected_RejectsLinkage()
        {
            _taxQueryRepo.Setup(r => r.GetMiscIdAsync(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Rejected)).ReturnsAsync(RejectedStatusId);
            _taxCommandRepo.Setup(r => r.RejectLinkageAsync(LinkageId, RejectedStatusId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            await CreateSut().Consume(Context(MiscEnumEntity.Rejected).Object);

            _taxCommandRepo.Verify(r => r.RejectLinkageAsync(LinkageId, RejectedStatusId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Consume_PendingStatus_DoesNothing()
        {
            // Strict mocks would throw if any repo were touched.
            await CreateSut().Consume(Context(MiscEnumEntity.Pending).Object);

            _taxCommandRepo.VerifyNoOtherCalls();
            _taxQueryRepo.VerifyNoOtherCalls();
        }
    }
}
