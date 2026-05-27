using Contracts.Dtos.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Inventory;
using FluentValidation.TestHelper;
using QCManagement.Application.Common.Interfaces.IQualityParameter;
using QCManagement.Presentation.Validation.QualityParameter;
using QCManagement.UnitTests.TestData;
using QCManagement.UnitTests.TestHelpers;

namespace QCManagement.UnitTests.Validators.QualityParameter
{
    public class CreateQualityParameterCommandValidatorTests
    {
        private readonly Mock<IQualityParameterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IUOMLookup> _mockUomLookup = new(MockBehavior.Loose);

        public CreateQualityParameterCommandValidatorTests()
        {
            _mockUomLookup.Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UOMLookupDto>());
        }

        private CreateQualityParameterCommandValidator CreateValidator()
            => new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object, _mockUomLookup.Object);

        private void SetupHappyPath(string name = "Yarn Tensile Strength", int groupId = 1, int dataTypeId = 2, int unitId = 12, int validationTypeId = 3, bool uomRequired = true)
        {
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(name, null)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.ParameterGroupExistsAsync(groupId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.DataTypeExistsAsync(dataTypeId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.ValidationTypeExistsAsync(validationTypeId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.IsUomRequiredForDataTypeAsync(dataTypeId)).ReturnsAsync(uomRequired);
            _mockUomLookup.Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UOMLookupDto> { new UOMLookupDto { Id = unitId, Code = "N", UOMName = "Newton" } });
        }

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            var command = QualityParameterBuilders.ValidCreateCommand();
            SetupHappyPath();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task ParameterName_Empty_FailsValidation(string? name)
        {
            var command = QualityParameterBuilders.ValidCreateCommand(name: name);
            SetupHappyPath(name: name ?? string.Empty);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ParameterName)
                  .WithErrorMessage("ParameterName is required.");
        }

        [Fact]
        public async Task ParameterName_TooLong_FailsValidation()
        {
            var longName = new string('A', 101);
            var command = QualityParameterBuilders.ValidCreateCommand(name: longName);
            SetupHappyPath(name: longName);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ParameterName)
                  .WithErrorMessage("ParameterName  cannot be longer than   100 characters.");
        }

        [Fact]
        public async Task ParameterName_AlreadyExists_FailsValidation()
        {
            var command = QualityParameterBuilders.ValidCreateCommand(name: "Duplicate");
            SetupHappyPath(name: "Duplicate");
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync("Duplicate", null)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ParameterName)
                  .WithErrorMessage("ParameterName already exists.");
        }

        [Fact]
        public async Task ParameterGroupId_NotInDb_FailsValidation()
        {
            var command = QualityParameterBuilders.ValidCreateCommand(parameterGroupId: 999);
            SetupHappyPath(groupId: 999);
            _mockQueryRepo.Setup(r => r.ParameterGroupExistsAsync(999)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ParameterGroupId);
        }

        [Fact]
        public async Task DataTypeId_NotInDb_FailsValidation()
        {
            var command = QualityParameterBuilders.ValidCreateCommand(dataTypeId: 999);
            SetupHappyPath(dataTypeId: 999, uomRequired: false);
            _mockQueryRepo.Setup(r => r.DataTypeExistsAsync(999)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.DataTypeId);
        }

        [Fact]
        public async Task UomRequired_ButMissing_FailsValidation()
        {
            var command = QualityParameterBuilders.ValidCreateCommand(unitId: null);
            SetupHappyPath(unitId: 0, uomRequired: true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.UnitId)
                  .WithErrorMessage("Unit of Measure is required for Numeric/Decimal data types, and must be empty otherwise.");
        }

        [Fact]
        public async Task UomNotRequired_ButProvided_FailsValidation()
        {
            var command = QualityParameterBuilders.ValidCreateCommand(dataTypeId: 3, unitId: 12);
            SetupHappyPath(dataTypeId: 3, unitId: 12, uomRequired: false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.UnitId)
                  .WithErrorMessage("Unit of Measure is required for Numeric/Decimal data types, and must be empty otherwise.");
        }

        [Fact]
        public async Task UnitId_NotInLookup_FailsValidation()
        {
            var command = QualityParameterBuilders.ValidCreateCommand(unitId: 999);
            SetupHappyPath(unitId: 999, uomRequired: true);
            _mockUomLookup.Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UOMLookupDto>());

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.UnitId)
                  .WithErrorMessage("UnitId does not exist in UOM Master.");
        }

        [Fact]
        public async Task Description_TooLong_FailsValidation()
        {
            var longDesc = new string('X', 501);
            var command = QualityParameterBuilders.ValidCreateCommand(description: longDesc);
            SetupHappyPath();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Description)
                  .WithErrorMessage("Description  cannot be longer than   500 characters.");
        }
    }
}
