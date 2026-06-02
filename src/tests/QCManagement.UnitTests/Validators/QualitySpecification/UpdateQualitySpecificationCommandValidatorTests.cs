using FluentValidation.TestHelper;
using QCManagement.Application.Common.Interfaces.IQualitySpecification;
using QCManagement.Application.QualitySpecification.Commands.UpdateQualitySpecification;
using QCManagement.Presentation.Validation.QualitySpecification;
using QCManagement.UnitTests.TestData;
using QCManagement.UnitTests.TestHelpers;

namespace QCManagement.UnitTests.Validators.QualitySpecification
{
    public class UpdateQualitySpecificationCommandValidatorTests
    {
        private readonly Mock<IQualitySpecificationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private UpdateQualitySpecificationCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        private void SetupHappyPath(UpdateQualitySpecificationCommand command)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(command.Id)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(command.SpecificationName!, command.Id)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.QcTypeExistsAsync(command.QcTypeId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.GetExistingParameterRowIdsAsync(command.Id))
                .ReturnsAsync(command.Parameters?.Select(p => p.Id).ToList() ?? new List<int>());
            _mockQueryRepo.Setup(r => r.GetSpecificationItemContextAsync(command.Id))
                .ReturnsAsync(((int?)null, (int?)null));
            _mockQueryRepo.Setup(r => r.HasOverlappingActiveSpecAsync(
                It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset?>(), command.Id))
                .ReturnsAsync(false);

            if (command.Parameters != null)
            {
                var typeIds = command.Parameters.Select(p => p.ValidationTypeId).Distinct().ToList();
                var codeMap = typeIds.ToDictionary(id => id, id => id switch
                {
                    QualitySpecificationBuilders.ValidationTypeRangeId => "RNG",
                    QualitySpecificationBuilders.ValidationTypeMinId => "MIN",
                    QualitySpecificationBuilders.ValidationTypeMaxId => "MAX",
                    QualitySpecificationBuilders.ValidationTypeFixedId => "FIX",
                    QualitySpecificationBuilders.ValidationTypePassFailId => "PFL",
                    QualitySpecificationBuilders.ValidationTypeListId => "LST",
                    _ => "RNG"
                });
                _mockQueryRepo.Setup(r => r.GetValidationTypeCodesByIdsAsync(It.IsAny<IEnumerable<int>>())).ReturnsAsync(codeMap);

                foreach (var p in command.Parameters)
                {
                    _mockQueryRepo.Setup(r => r.ValidationTypeExistsAsync(p.ValidationTypeId)).ReturnsAsync(true);
                    if (p.SeverityId.HasValue && p.SeverityId.Value > 0)
                        _mockQueryRepo.Setup(r => r.SeverityExistsAsync(p.SeverityId.Value)).ReturnsAsync(true);
                    if (p.FailureActionId.HasValue && p.FailureActionId.Value > 0)
                        _mockQueryRepo.Setup(r => r.FailureActionExistsAsync(p.FailureActionId.Value)).ReturnsAsync(true);
                }
            }
        }

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            var command = QualitySpecificationBuilders.ValidUpdateCommand();
            SetupHappyPath(command);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task NonExistentId_FailsValidation()
        {
            var command = QualitySpecificationBuilders.ValidUpdateCommand(id: 99);
            SetupHappyPath(command);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task IsActive_OutOfRange_FailsValidation()
        {
            var command = QualitySpecificationBuilders.ValidUpdateCommand(isActive: 5);
            SetupHappyPath(command);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.IsActive);
        }

        [Fact]
        public async Task ParameterIdsDontMatchExisting_FailsValidation()
        {
            var command = QualitySpecificationBuilders.ValidUpdateCommand();
            SetupHappyPath(command);
            _mockQueryRepo.Setup(r => r.GetExistingParameterRowIdsAsync(command.Id))
                .ReturnsAsync(new List<int> { 99 });  // DB has different Id

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x);
        }

        [Fact]
        public async Task EffectiveTo_BeforeEffectiveFrom_FailsValidation()
        {
            var from = new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var to = from.AddDays(-1);
            var command = QualitySpecificationBuilders.ValidUpdateCommand(effectiveFrom: from, effectiveTo: to);
            SetupHappyPath(command);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.EffectiveTo);
        }
    }
}
