using Contracts.Dtos.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Inventory;
using FluentValidation.TestHelper;
using QCManagement.Application.Common.Interfaces.IQualityTemplate;
using QCManagement.Application.QualityTemplate.Commands.UpdateQualityTemplate;
using QCManagement.Presentation.Validation.QualityTemplate;
using QCManagement.UnitTests.TestData;
using QCManagement.UnitTests.TestHelpers;

namespace QCManagement.UnitTests.Validators.QualityTemplate
{
    public class UpdateQualityTemplateCommandValidatorTests
    {
        private readonly Mock<IQualityTemplateQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IUOMLookup> _mockUomLookup = new(MockBehavior.Loose);

        public UpdateQualityTemplateCommandValidatorTests()
        {
            _mockUomLookup
                .Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                    ids.Select(id => new UOMLookupDto { Id = id, Code = "KG", UOMName = "Kilogram" }).ToList());
        }

        private UpdateQualityTemplateCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object, _mockUomLookup.Object);

        private void SetupHappyPath(UpdateQualityTemplateCommand command)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(command.Id)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(command.TemplateName!, command.Id)).ReturnsAsync(false);
            if (command.Parameters != null)
            {
                foreach (var p in command.Parameters)
                {
                    _mockQueryRepo.Setup(r => r.QualityParameterExistsAndActiveAsync(p.QualityParameterId)).ReturnsAsync(true);
                    if (p.InspectionMethodId.HasValue && p.InspectionMethodId.Value > 0)
                        _mockQueryRepo.Setup(r => r.InspectionMethodExistsAsync(p.InspectionMethodId.Value)).ReturnsAsync(true);
                }
            }
        }

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            var command = QualityTemplateBuilders.ValidUpdateCommand();
            SetupHappyPath(command);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task NonExistentId_FailsValidation()
        {
            var command = QualityTemplateBuilders.ValidUpdateCommand(id: 999);
            SetupHappyPath(command);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(999)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task IsActiveInvalid_FailsValidation()
        {
            var command = QualityTemplateBuilders.ValidUpdateCommand(isActive: 5);
            SetupHappyPath(command);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.IsActive);
        }

        [Fact]
        public async Task EmptyParameterList_FailsValidation()
        {
            var command = QualityTemplateBuilders.ValidUpdateCommand(parameters: new List<UpdateQualityTemplateParameterDto>());
            SetupHappyPath(command);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Parameters);
        }

        [Fact]
        public async Task DuplicateQualityParameterId_FailsValidation()
        {
            var dupList = new List<UpdateQualityTemplateParameterDto>
            {
                new() { QualityParameterId = 1, SequenceNo = 1, InspectionMethodId = 17, IsActive = 1 },
                new() { QualityParameterId = 1, SequenceNo = 2, InspectionMethodId = 18, IsActive = 1 }
            };
            var command = QualityTemplateBuilders.ValidUpdateCommand(parameters: dupList);
            SetupHappyPath(command);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Parameters)
                  .WithErrorMessage("Duplicate parameters within the same template are not allowed.");
        }
    }
}
