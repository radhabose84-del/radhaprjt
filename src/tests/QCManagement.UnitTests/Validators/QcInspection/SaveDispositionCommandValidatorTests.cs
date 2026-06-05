using FluentValidation.TestHelper;
using QCManagement.Application.Common.Interfaces.IQcInspection;
using QCManagement.Application.Common.Services;
using QCManagement.Application.QcInspection.Commands.SaveDisposition;
using QCManagement.Application.QcInspection.Commands.SaveParameterCollection;
using QCManagement.Application.QcInspection.Dto;
using QCManagement.Presentation.Validation.QcInspection;
using QCManagement.UnitTests.TestData;
using QCManagement.UnitTests.TestHelpers;

namespace QCManagement.UnitTests.Validators.QcInspection
{
    public sealed class SaveDispositionCommandValidatorTests
    {
        private readonly Mock<IQcInspectionQueryRepository> _qry = new(MockBehavior.Strict);
        private readonly Mock<IInspectionEvaluator> _eval = new(MockBehavior.Loose);

        private SaveDispositionCommandValidator CreateValidator() =>
            new(_qry.Object, TestMaxLengthProviderFactory.Create(), _eval.Object);

        // hdr 88, one parameter row (Id 11), received qty 2, status 34 = APR.
        private void SetupValid(string severityCode = "MIN")
        {
            _qry.Setup(q => q.NotFoundAsync(88)).ReturnsAsync(false);
            _qry.Setup(q => q.GetDetailEvaluationRowsAsync(88)).ReturnsAsync(new List<QcInspectionDtlEvalDto>
            {
                new() { Id = 11, ValidationTypeCode = "RNG", MinValue = 10m, MaxValue = 50m, SeverityCode = severityCode }
            });
            _qry.Setup(q => q.QcStatusIdExistsAsync(34)).ReturnsAsync(true);
            _qry.Setup(q => q.GetReceivedQuantityAsync(88)).ReturnsAsync(2m);
            _qry.Setup(q => q.GetQcStatusCodeByIdAsync(34)).ReturnsAsync("APR");
        }

        private static SaveDispositionCommand Cmd(List<ParameterResultInputDto>? parameters = null) =>
            QcInspectionBuilders.ValidDispositionCommand(
                hdrId: 88, qcStatusId: 34, acc: 2m, rej: 0m,
                parameters: parameters ?? new List<ParameterResultInputDto>
                {
                    new() { DetailId = 11, ActualValue = "30", Remarks = "ok" }
                });

        [Fact]
        public async Task Valid_Passes()
        {
            SetupValid();
            var result = await CreateValidator().TestValidateAsync(Cmd());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task EmptyParameters_Fails()
        {
            SetupValid();
            var result = await CreateValidator().TestValidateAsync(Cmd(parameters: new List<ParameterResultInputDto>()));
            result.ShouldHaveValidationErrorFor(x => x.Parameters);
        }

        [Fact]
        public async Task MissingActualValue_Fails()
        {
            SetupValid();
            var result = await CreateValidator().TestValidateAsync(Cmd(parameters: new List<ParameterResultInputDto>
            {
                new() { DetailId = 11, ActualValue = null, Remarks = "ok" }
            }));
            result.ShouldHaveValidationErrorFor("Parameters[0].ActualValue");
        }

        [Fact]
        public async Task NotAllRowsCovered_Fails()
        {
            SetupValid();
            _qry.Setup(q => q.GetDetailEvaluationRowsAsync(88)).ReturnsAsync(new List<QcInspectionDtlEvalDto>
            {
                new() { Id = 11, ValidationTypeCode = "RNG", SeverityCode = "MIN" },
                new() { Id = 12, ValidationTypeCode = "PFL", SeverityCode = "MIN" }
            });

            var result = await CreateValidator().TestValidateAsync(Cmd());

            result.ShouldHaveValidationErrorFor(x => x)
                  .WithErrorMessage("All parameters must be evaluated before disposition.");
        }

        [Fact]
        public async Task ApprovedWithCriticalFail_Fails()
        {
            SetupValid(severityCode: "CRT");
            _eval.Setup(e => e.Evaluate(
                    It.IsAny<string?>(), It.IsAny<string?>(),
                    It.IsAny<decimal?>(), It.IsAny<decimal?>(), It.IsAny<string?>(), It.IsAny<string?>()))
                .Returns("FAIL");

            var result = await CreateValidator().TestValidateAsync(Cmd());

            result.ShouldHaveValidationErrorFor(x => x)
                  .WithErrorMessage("Cannot approve — critical parameter(s) failed.");
        }
    }
}
