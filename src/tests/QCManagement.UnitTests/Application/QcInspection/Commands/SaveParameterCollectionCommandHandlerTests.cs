using QCManagement.Application.Common.Interfaces.IQcInspection;
using QCManagement.Application.Common.Services;
using QCManagement.Application.QcInspection.Commands.SaveParameterCollection;
using QCManagement.Application.QcInspection.Dto;
using QCManagement.UnitTests.TestData;

namespace QCManagement.UnitTests.Application.QcInspection.Commands
{
    public class SaveParameterCollectionCommandHandlerTests
    {
        private readonly Mock<IQcInspectionCommandRepository> _cmd = new(MockBehavior.Strict);
        private readonly Mock<IQcInspectionQueryRepository> _qry = new(MockBehavior.Strict);
        private readonly IInspectionEvaluator _evaluator = new InspectionEvaluator();
        private readonly Mock<IMediator> _mediator = new(MockBehavior.Strict);

        private SaveParameterCollectionCommandHandler CreateSut() =>
            new(_cmd.Object, _qry.Object, _evaluator, _mediator.Object);

        [Fact]
        public async Task Handle_EvaluatesAndPersists()
        {
            IReadOnlyList<(int DetailId, string? ActualValue, string? InspectionResult, string? Remarks)>? captured = null;

            _qry.Setup(q => q.GetDetailEvaluationRowsAsync(88))
                .ReturnsAsync(new List<QcInspectionDtlEvalDto>
                {
                    new() { Id = 501, ValidationTypeCode = "RNG", MinValue = 10m, MaxValue = 50m }
                });

            _cmd.Setup(c => c.SaveParameterResultsAsync(88,
                    It.IsAny<IReadOnlyList<(int, string?, string?, string?)>>()))
                .Callback<int, IReadOnlyList<(int, string?, string?, string?)>>((_, list) => captured = list)
                .ReturnsAsync(1);

            _mediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(QcInspectionBuilders.ValidParamsCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            captured.Should().NotBeNull();
            captured!.Single().InspectionResult.Should().Be("PASS"); // 40 within [10,50]
        }

        [Fact]
        public async Task Handle_OutOfRange_ComputesFail()
        {
            IReadOnlyList<(int DetailId, string? ActualValue, string? InspectionResult, string? Remarks)>? captured = null;

            _qry.Setup(q => q.GetDetailEvaluationRowsAsync(88))
                .ReturnsAsync(new List<QcInspectionDtlEvalDto>
                {
                    new() { Id = 501, ValidationTypeCode = "RNG", MinValue = 10m, MaxValue = 50m }
                });
            _cmd.Setup(c => c.SaveParameterResultsAsync(88,
                    It.IsAny<IReadOnlyList<(int, string?, string?, string?)>>()))
                .Callback<int, IReadOnlyList<(int, string?, string?, string?)>>((_, list) => captured = list)
                .ReturnsAsync(1);
            _mediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var cmd = QcInspectionBuilders.ValidParamsCommand();
            cmd.Parameters[0].ActualValue = "99";

            await CreateSut().Handle(cmd, CancellationToken.None);

            captured!.Single().InspectionResult.Should().Be("FAIL");
        }
    }
}
