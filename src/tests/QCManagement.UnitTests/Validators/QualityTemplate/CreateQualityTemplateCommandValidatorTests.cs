using Contracts.Dtos.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Inventory;
using FluentValidation.TestHelper;
using QCManagement.Application.Common.Interfaces.IQualityTemplate;
using QCManagement.Application.QualityTemplate.Commands.CreateQualityTemplate;
using QCManagement.Presentation.Validation.QualityTemplate;
using QCManagement.UnitTests.TestData;
using QCManagement.UnitTests.TestHelpers;

namespace QCManagement.UnitTests.Validators.QualityTemplate
{
    public class CreateQualityTemplateCommandValidatorTests
    {
        private readonly Mock<IQualityTemplateQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IUOMLookup> _mockUomLookup = new(MockBehavior.Loose);

        public CreateQualityTemplateCommandValidatorTests()
        {
            // Default lookup: always returns the requested ids as found
            _mockUomLookup
                .Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                    ids.Select(id => new UOMLookupDto { Id = id, Code = "KG", UOMName = "Kilogram" }).ToList());
        }

        private CreateQualityTemplateCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object, _mockUomLookup.Object);

        private void SetupHappyPath(CreateQualityTemplateCommand command)
        {
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(command.TemplateName!, null)).ReturnsAsync(false);
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
            var command = QualityTemplateBuilders.ValidCreateCommand();
            SetupHappyPath(command);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task TemplateName_Empty_FailsValidation(string? name)
        {
            var command = QualityTemplateBuilders.ValidCreateCommand(name: name);
            SetupHappyPath(command);
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(name ?? string.Empty, null)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.TemplateName);
        }

        [Fact]
        public async Task TemplateName_TooLong_FailsValidation()
        {
            var longName = new string('A', 101);
            var command = QualityTemplateBuilders.ValidCreateCommand(name: longName);
            SetupHappyPath(command);
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(longName, null)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.TemplateName);
        }

        [Fact]
        public async Task TemplateName_Duplicate_FailsValidation()
        {
            var command = QualityTemplateBuilders.ValidCreateCommand(name: "Existing Name");
            SetupHappyPath(command);
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync("Existing Name", null)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.TemplateName)
                  .WithErrorMessage("TemplateName already exists.");
        }

        [Fact]
        public async Task EmptyParameterList_FailsValidation()
        {
            var command = QualityTemplateBuilders.ValidCreateCommand(parameters: new List<CreateQualityTemplateParameterDto>());
            SetupHappyPath(command);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Parameters);
        }

        [Fact]
        public async Task DuplicateQualityParameterId_FailsValidation()
        {
            var dupList = new List<CreateQualityTemplateParameterDto>
            {
                new() { QualityParameterId = 1, SequenceNo = 1, InspectionMethodId = 17 },
                new() { QualityParameterId = 1, SequenceNo = 2, InspectionMethodId = 18 }
            };
            var command = QualityTemplateBuilders.ValidCreateCommand(parameters: dupList);
            SetupHappyPath(command);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Parameters)
                  .WithErrorMessage("Duplicate parameters within the same template are not allowed.");
        }

        [Fact]
        public async Task InactiveQualityParameter_FailsValidation()
        {
            var command = QualityTemplateBuilders.ValidCreateCommand();
            SetupHappyPath(command);
            _mockQueryRepo.Setup(r => r.QualityParameterExistsAndActiveAsync(1)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor("Parameters[0].QualityParameterId");
        }

        [Fact]
        public async Task SampleSizeWithoutUom_FailsValidation()
        {
            var command = QualityTemplateBuilders.ValidCreateCommand();
            command.Parameters![0].SampleSize = 5;
            command.Parameters![0].SampleUomId = null;
            SetupHappyPath(command);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor("Parameters[0].SampleUomId");
        }

        [Fact]
        public async Task NoSampleSize_PassesEvenWithoutUom()
        {
            var command = QualityTemplateBuilders.ValidCreateCommand();
            // Already valid params: second item has no SampleSize and no SampleUomId
            SetupHappyPath(command);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task SequenceNoZero_FailsValidation()
        {
            var command = QualityTemplateBuilders.ValidCreateCommand();
            command.Parameters![0].SequenceNo = 0;
            SetupHappyPath(command);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor("Parameters[0].SequenceNo");
        }

        [Fact]
        public async Task InvalidInspectionMethodId_FailsValidation()
        {
            var command = QualityTemplateBuilders.ValidCreateCommand();
            command.Parameters![0].InspectionMethodId = 999;
            SetupHappyPath(command);
            _mockQueryRepo.Setup(r => r.InspectionMethodExistsAsync(999)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor("Parameters[0].InspectionMethodId");
        }

        [Fact]
        public async Task InvalidSampleUomId_FailsValidation()
        {
            var command = QualityTemplateBuilders.ValidCreateCommand();
            command.Parameters![0].SampleUomId = 999;
            SetupHappyPath(command);
            // Override lookup to return empty list (Uom 999 not found)
            _mockUomLookup
                .Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UOMLookupDto>());

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Parameters);
        }
    }
}
