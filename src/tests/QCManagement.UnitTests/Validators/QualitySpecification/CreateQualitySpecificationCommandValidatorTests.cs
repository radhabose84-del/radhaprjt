using Contracts.Dtos.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Inventory;
using FluentValidation.TestHelper;
using QCManagement.Application.Common.Interfaces.IQualitySpecification;
using QCManagement.Application.Common.Interfaces.IQualityTemplate;
using QCManagement.Application.QualitySpecification.Commands.CreateQualitySpecification;
using QCManagement.Application.QualityTemplate.Dto;
using QCManagement.Presentation.Validation.QualitySpecification;
using QCManagement.UnitTests.TestData;
using QCManagement.UnitTests.TestHelpers;

namespace QCManagement.UnitTests.Validators.QualitySpecification
{
    public class CreateQualitySpecificationCommandValidatorTests
    {
        private readonly Mock<IQualitySpecificationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IQualityTemplateQueryRepository> _mockTemplateRepo = new(MockBehavior.Strict);
        private readonly Mock<IInventoryCategoryLookup> _mockCategoryLookup = new(MockBehavior.Loose);
        private readonly Mock<IItemLookup> _mockItemLookup = new(MockBehavior.Loose);

        public CreateQualitySpecificationCommandValidatorTests()
        {
            // Default lookups: always return what's requested
            _mockCategoryLookup
                .Setup(c => c.GetCategoryByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                    ids.Select(id => new CategoryMasterDto { Id = id, ItemCategoryName = $"Cat{id}" }).ToList());

            _mockItemLookup
                .Setup(i => i.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                    ids.Select(id => new ItemLookupDto { Id = id, ItemCode = $"I{id}", ItemName = $"Item{id}" }).ToList());
        }

        private CreateQualitySpecificationCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object, _mockTemplateRepo.Object, _mockCategoryLookup.Object, _mockItemLookup.Object);

        private void SetupHappyPath(CreateQualitySpecificationCommand command)
        {
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(command.SpecificationName!, null)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.QcTypeExistsAsync(command.QcTypeId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.ApplicableLevelExistsAsync(command.ApplicableLevelId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.GetApplicableLevelCodeAsync(command.ApplicableLevelId)).ReturnsAsync("ITEM CATEGORY");
            _mockQueryRepo.Setup(r => r.QcTypeExistsAsync(command.QcTypeId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.HasOverlappingActiveSpecAsync(
                It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset?>(), null))
                .ReturnsAsync(false);

            // Template returns a DTO whose parameters set matches the command's parameter set
            var tpl = new QualityTemplateDto
            {
                Id = command.QualityTemplateId,
                IsActive = true,
                Parameters = command.Parameters?.Select(p => new QualityTemplateParameterDto
                {
                    Id = p.QualityParameterId,
                    QualityParameterId = p.QualityParameterId,
                    IsActive = true
                }).ToList()
            };
            _mockTemplateRepo.Setup(r => r.GetByIdAsync(command.QualityTemplateId)).ReturnsAsync(tpl);

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
            var command = QualitySpecificationBuilders.ValidCreateCommand();
            SetupHappyPath(command);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task SpecificationName_Empty_FailsValidation(string? name)
        {
            var command = QualitySpecificationBuilders.ValidCreateCommand(name: name);
            SetupHappyPath(command);
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(name ?? string.Empty, null)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SpecificationName);
        }

        [Fact]
        public async Task SpecificationName_Duplicate_FailsValidation()
        {
            var command = QualitySpecificationBuilders.ValidCreateCommand(name: "Existing Name");
            SetupHappyPath(command);
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync("Existing Name", null)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SpecificationName);
        }

        [Fact]
        public async Task EmptyParameterList_FailsValidation()
        {
            var command = QualitySpecificationBuilders.ValidCreateCommand(parameters: new List<CreateQualitySpecificationParameterDto>());
            SetupHappyPath(command);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Parameters);
        }

        [Fact]
        public async Task EffectiveTo_BeforeEffectiveFrom_FailsValidation()
        {
            var from = new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var to = from.AddDays(-1);
            var command = QualitySpecificationBuilders.ValidCreateCommand(effectiveFrom: from, effectiveTo: to);
            SetupHappyPath(command);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.EffectiveTo);
        }

        [Fact]
        public async Task ApplicableLevelCategory_MissingItemCategoryId_FailsValidation()
        {
            var command = QualitySpecificationBuilders.ValidCreateCommand(
                applicableLevelId: QualitySpecificationBuilders.ApplicableLevelItemCategoryId,
                itemCategoryId: null, itemId: null);
            SetupHappyPath(command);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x);
        }

        [Fact]
        public async Task Range_MinGreaterThanMax_FailsValidation()
        {
            var command = QualitySpecificationBuilders.ValidCreateCommand();
            command.Parameters![0].MinValue = 50m;
            command.Parameters![0].MaxValue = 40m;
            SetupHappyPath(command);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Parameters);
        }

        [Fact]
        public async Task ListSelection_NoAllowedValues_FailsValidation()
        {
            var command = QualitySpecificationBuilders.ValidCreateCommand();
            var listParam = command.Parameters!.First(p => p.ValidationTypeId == QualitySpecificationBuilders.ValidationTypeListId);
            listParam.AllowedValues = new List<string>();
            SetupHappyPath(command);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Parameters);
        }

        [Fact]
        public async Task ListSelection_ValueContainsPipe_FailsValidation()
        {
            var command = QualitySpecificationBuilders.ValidCreateCommand();
            var listParam = command.Parameters!.First(p => p.ValidationTypeId == QualitySpecificationBuilders.ValidationTypeListId);
            listParam.AllowedValues = new List<string> { "A", "B|C" };
            SetupHappyPath(command);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Parameters);
        }

        [Fact]
        public async Task ListSelection_DuplicateValues_FailsValidation()
        {
            var command = QualitySpecificationBuilders.ValidCreateCommand();
            var listParam = command.Parameters!.First(p => p.ValidationTypeId == QualitySpecificationBuilders.ValidationTypeListId);
            listParam.AllowedValues = new List<string> { "A", "a" };
            SetupHappyPath(command);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Parameters);
        }

        [Fact]
        public async Task PassFail_WithMinMaxProvided_FailsValidation()
        {
            var command = QualitySpecificationBuilders.ValidCreateCommand(
                parameters: new List<CreateQualitySpecificationParameterDto>
                {
                    new CreateQualitySpecificationParameterDto
                    {
                        QualityParameterId = 1,
                        ValidationTypeId = QualitySpecificationBuilders.ValidationTypePassFailId,
                        MinValue = 1m  // illegal for PFL
                    }
                });
            SetupHappyPath(command);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Parameters);
        }

        [Fact]
        public async Task OverlappingActiveSpec_FailsValidation()
        {
            var command = QualitySpecificationBuilders.ValidCreateCommand();
            SetupHappyPath(command);
            _mockQueryRepo.Setup(r => r.HasOverlappingActiveSpecAsync(
                It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset?>(), null))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x);
        }
    }
}
