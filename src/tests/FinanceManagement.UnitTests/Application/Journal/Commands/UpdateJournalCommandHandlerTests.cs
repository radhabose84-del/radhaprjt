using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal;
using FinanceManagement.Application.JournalMaster.Journal.Commands.UpdateJournal;
using FinanceManagement.UnitTests.TestData;

namespace FinanceManagement.UnitTests.Application.Journal.Commands
{
    public sealed class UpdateJournalCommandHandlerTests
    {
        private readonly Mock<IJournalCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IJournalQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateJournalCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockIp.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int updatedId = 1)
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            _mockQueryRepo.Setup(r => r.GetOpenPeriodByDateAsync(1, It.IsAny<DateOnly>()))
                .ReturnsAsync(((int, int)?)(4, 3));
            _mockMapper.Setup(m => m.Map<FinanceManagement.Domain.Entities.JournalHeader>(It.IsAny<UpdateJournalCommand>()))
                .Returns(new FinanceManagement.Domain.Entities.JournalHeader());
            _mockCommandRepo.Setup(r => r.UpdateAsync(It.IsAny<FinanceManagement.Domain.Entities.JournalHeader>()))
                .ReturnsAsync(updatedId);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(JournalBuilders.ValidUpdateCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_RecomputesTotals()
        {
            FinanceManagement.Domain.Entities.JournalHeader? captured = null;
            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            _mockQueryRepo.Setup(r => r.GetOpenPeriodByDateAsync(1, It.IsAny<DateOnly>()))
                .ReturnsAsync(((int, int)?)(4, 3));
            _mockMapper.Setup(m => m.Map<FinanceManagement.Domain.Entities.JournalHeader>(It.IsAny<UpdateJournalCommand>()))
                .Returns(new FinanceManagement.Domain.Entities.JournalHeader());
            _mockCommandRepo.Setup(r => r.UpdateAsync(It.IsAny<FinanceManagement.Domain.Entities.JournalHeader>()))
                .Callback<FinanceManagement.Domain.Entities.JournalHeader>(e => captured = e)
                .ReturnsAsync(1);

            var cmd = JournalBuilders.ValidUpdateCommand(lines: new List<FinanceManagement.Application.JournalMaster.Dto.JournalLineInputDto>
            {
                JournalBuilders.DrLine(amount: 2500m),
                JournalBuilders.CrLine(amount: 2500m)
            });

            await CreateSut().Handle(cmd, CancellationToken.None);

            captured!.TotalDr.Should().Be(2500m);
            captured.TotalCr.Should().Be(2500m);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(JournalBuilders.ValidUpdateCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "Update" && e.ActionCode == "JOURNAL_UPDATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
