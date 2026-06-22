using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Interfaces.ICoaChangeRequest;
using FinanceManagement.Application.CoaChangeRequest.Commands.CreateCoaChangeRequest;
using FinanceManagement.Domain.Entities;

namespace FinanceManagement.UnitTests.Application.CoaChangeRequest.Commands
{
    public sealed class CreateCoaChangeRequestCommandHandlerTests
    {
        private readonly Mock<ICoaChangeRequestCommandRepository> _mockCmd = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService> _mockTz = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private CreateCoaChangeRequestCommandHandler CreateSut() =>
            new(_mockCmd.Object, _mockIp.Object, _mockTz.Object, _mockMediator.Object);

        private static CreateCoaChangeRequestCommand ValidCommand() => new()
        {
            TargetAccountId = 5,
            ChangeType = CoaChangeType.AccountEdit,
            Justification = "year-end correction",
            ImpactAssessment = "no downstream impact"
        };

        private void SetupSession()
        {
            _mockIp.Setup(x => x.GetCompanyId()).Returns(1);
            _mockIp.Setup(x => x.GetUserId()).Returns(3);
            _mockTz.Setup(x => x.GetCurrentTime(It.IsAny<string>())).Returns(new DateTimeOffset(2026, 6, 22, 10, 0, 0, TimeSpan.Zero));
        }

        [Fact]
        public async Task Handle_ValidCommand_PersistsPendingImpactApprovalAndReturnsId()
        {
            SetupSession();
            _mockCmd.Setup(r => r.AddChangeRequestWithoutSaveAsync(It.IsAny<FinanceManagement.Domain.Entities.CoaChangeRequest>(), It.IsAny<CancellationToken>()))
                .Callback<FinanceManagement.Domain.Entities.CoaChangeRequest, CancellationToken>((e, _) => e.Id = 77)
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Be(77);
        }

        [Fact]
        public async Task Handle_ValidCommand_SetsStatusAndRequester()
        {
            SetupSession();
            FinanceManagement.Domain.Entities.CoaChangeRequest? captured = null;
            _mockCmd.Setup(r => r.AddChangeRequestWithoutSaveAsync(It.IsAny<FinanceManagement.Domain.Entities.CoaChangeRequest>(), It.IsAny<CancellationToken>()))
                .Callback<FinanceManagement.Domain.Entities.CoaChangeRequest, CancellationToken>((e, _) => captured = e)
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            captured.Should().NotBeNull();
            captured!.RequestStatus.Should().Be(CoaChangeRequestStatus.PendingImpactApproval);
            captured.CompanyId.Should().Be(1);
            captured.RequestedByUserId.Should().Be(3);
            captured.IsPostFreeze.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupSession();
            _mockCmd.Setup(r => r.AddChangeRequestWithoutSaveAsync(It.IsAny<FinanceManagement.Domain.Entities.CoaChangeRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "COA_CHANGE_REQUEST_CREATE"),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
