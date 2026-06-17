using Contracts.Commands.Workflow;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IOutbox;
using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Application.TaxCode.Commands.SubmitLinkageChangeRequest;

namespace FinanceManagement.UnitTests.Application.TaxCode.Commands
{
    public sealed class SubmitLinkageChangeRequestCommandHandlerTests
    {
        private readonly Mock<ITaxCodeCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<ITaxCodeQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IOutboxEventPublisher> _mockOutbox = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private const int PendingStatusId = 19;

        public SubmitLinkageChangeRequestCommandHandlerTests()
        {
            _mockIp.Setup(x => x.GetCompanyId()).Returns(1);
            _mockIp.Setup(x => x.GetUnitId()).Returns(5);
            _mockQueryRepo.Setup(r => r.GetMiscIdAsync("ApprovalStatus", "PENDING")).ReturnsAsync(PendingStatusId);
            _mockQueryRepo.Setup(r => r.GetLinkageByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new FinanceManagement.Application.TaxCode.Dto.TaxAccountLinkageDto { Id = 20, GlAccountId = 412 });
            // current active linkage being superseded (provides Old* values)
            _mockQueryRepo.Setup(r => r.GetLinkageByAccountAsync(It.IsAny<int>()))
                .ReturnsAsync(new FinanceManagement.Application.TaxCode.Dto.TaxAccountLinkageDto { Id = 9, GlAccountId = 412, TaxCodeId = 14, ControlAccountId = 29 });
        }

        private SubmitLinkageChangeRequestCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockIp.Object, _mockOutbox.Object, _mockMediator.Object);

        private static SubmitLinkageChangeRequestCommand ValidCommand() =>
            new()
            {
                GlAccountId = 412,
                NewTaxCodeId = 15,
                NewControlAccountId = 30,
                Reason = "HSN reclassification pending review",
                EffectiveFrom = new DateOnly(2026, 5, 25)
            };

        [Fact]
        public async Task Handle_ValidCommand_CreatesPendingLinkageRow()
        {
            _mockCommandRepo
                .Setup(r => r.CreateLinkageAsync(It.IsAny<FinanceManagement.Domain.Entities.TaxAccountLinkage>()))
                .ReturnsAsync(20);

            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Be(20);
            _mockCommandRepo.Verify(
                r => r.CreateLinkageAsync(It.Is<FinanceManagement.Domain.Entities.TaxAccountLinkage>(
                    l => l.TaxCodeId == 15 && l.GlAccountId == 412 && l.StatusId == PendingStatusId
                        && l.OldTaxLinkageId == 9
                        && l.IsActive == FinanceManagement.Domain.Common.BaseEntity.Status.Inactive)),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesChangeRequestAudit()
        {
            _mockCommandRepo
                .Setup(r => r.CreateLinkageAsync(It.IsAny<FinanceManagement.Domain.Entities.TaxAccountLinkage>()))
                .ReturnsAsync(20);

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "TAX_ACCOUNT_LINKAGE_CHANGE_REQUEST"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_SchedulesApprovalRequestToOutbox()
        {
            _mockCommandRepo
                .Setup(r => r.CreateLinkageAsync(It.IsAny<FinanceManagement.Domain.Entities.TaxAccountLinkage>()))
                .ReturnsAsync(20);

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockOutbox.Verify(
                o => o.ScheduleAsync(
                    It.Is<CreateApprovalRequestCommand>(c =>
                        c.ModuleTransactionId == 20 &&
                        c.ModuleTypeName == FinanceManagement.Domain.Common.MiscEnumEntity.TaxAccountLinkage),
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
