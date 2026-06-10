using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Purchase;
using QCManagement.Application.Common.Interfaces.IQcInspection;
using QCManagement.Application.Common.Services;
using QCManagement.Application.QcInspection.Commands.SaveDisposition;
using QCManagement.Application.QcInspection.Dto;
using QCManagement.Domain.Events;
using QCManagement.UnitTests.TestData;

namespace QCManagement.UnitTests.Application.QcInspection.Commands
{
    public class SaveDispositionCommandHandlerTests
    {
        private readonly Mock<IQcInspectionCommandRepository> _cmd = new(MockBehavior.Strict);
        private readonly Mock<IQcInspectionQueryRepository> _qry = new(MockBehavior.Strict);
        private readonly Mock<IInspectionEvaluator> _eval = new(MockBehavior.Loose);
        private readonly Mock<IGrnLookup> _grn = new(MockBehavior.Strict);
        private readonly Mock<IArrivalLookup> _arrival = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mediator = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _ip = new(MockBehavior.Loose);

        private SaveDispositionCommandHandler CreateSut() =>
            new(_cmd.Object, _qry.Object, _eval.Object, _grn.Object, _arrival.Object, _mediator.Object, _ip.Object);

        private void SetupHappyPath()
        {
            _qry.Setup(q => q.GetQcStatusCodeByIdAsync(34)).ReturnsAsync("APR");
            _qry.Setup(q => q.GetDispositionContextAsync(88)).ReturnsAsync(QcInspectionBuilders.ValidContext());
            _qry.Setup(q => q.GetSourceTypeIdByCodeAsync("ARRIVAL"))
                .ReturnsAsync(QcInspectionBuilders.ArrivalSourceTypeId);
            _qry.Setup(q => q.GetDetailEvaluationRowsAsync(88)).ReturnsAsync(new List<QcInspectionDtlEvalDto>
            {
                new() { Id = 11, ValidationTypeCode = "RNG", MinValue = 10m, MaxValue = 50m, SeverityCode = "CRT" }
            });
            _eval.Setup(e => e.Evaluate(
                    It.IsAny<string?>(), It.IsAny<string?>(),
                    It.IsAny<decimal?>(), It.IsAny<decimal?>(), It.IsAny<string?>(), It.IsAny<string?>()))
                .Returns("PASS");
            // GRN source → sourceTypeCode "GRN", header 100, detail 4321, mapped arrival status "Approved".
            _cmd.Setup(c => c.SaveResultsAndDispositionAsync(88,
                    It.IsAny<IReadOnlyList<(int, string?, string?, string?)>>(),
                    34, 1000m, 0m, null,
                    It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(), true, "GRN", 100, 4321, "Approved"))
                .ReturnsAsync(88);
            _grn.Setup(g => g.GetByGrnDetailIdAsync(4321, It.IsAny<CancellationToken>()))
                .ReturnsAsync(QcInspectionBuilders.ValidGrnLookup());
            _mediator.Setup(m => m.Publish(It.IsAny<QcDispositionCompletedDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _mediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_Valid_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(QcInspectionBuilders.ValidDispositionCommand(), CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_Valid_PersistsResultsAndDisposition()
        {
            SetupHappyPath();
            await CreateSut().Handle(QcInspectionBuilders.ValidDispositionCommand(), CancellationToken.None);

            _cmd.Verify(c => c.SaveResultsAndDispositionAsync(88,
                It.IsAny<IReadOnlyList<(int, string?, string?, string?)>>(),
                34, 1000m, 0m, null,
                It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(), true, "GRN", 100, 4321, "Approved"), Times.Once);
        }

        [Fact]
        public async Task Handle_Valid_PublishesMovementEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(QcInspectionBuilders.ValidDispositionCommand(), CancellationToken.None);

            _mediator.Verify(m => m.Publish(
                It.Is<QcDispositionCompletedDomainEvent>(e => e.QcStatusId == 34 && e.QcStatusCode == "APR" && e.SourceHeaderId == 100),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
