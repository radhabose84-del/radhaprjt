using Contracts.Dtos.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Inventory;
using FluentValidation.TestHelper;
using QCManagement.Application.Common.Interfaces.IQualityParameter;
using QCManagement.Presentation.Validation.QualityParameter;
using QCManagement.UnitTests.TestData;
using QCManagement.UnitTests.TestHelpers;

namespace QCManagement.UnitTests.Validators.QualityParameter
{
    public class UpdateQualityParameterCommandValidatorTests
    {
        private readonly Mock<IQualityParameterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IUOMLookup> _mockUomLookup = new(MockBehavior.Loose);

        public UpdateQualityParameterCommandValidatorTests()
        {
            // Default Loose return — tests can override with specific setup.
            _mockUomLookup.Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UOMLookupDto>());
        }

        private UpdateQualityParameterCommandValidator CreateValidator()
            => new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object, _mockUomLookup.Object);

        private void SetupHappyPath(int id = 1, string name = "Updated Tensile Strength", int groupId = 1, int unitId = 12, bool uomRequired = true)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(name, id)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.ParameterGroupExistsAsync(groupId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.GetDataTypeIdByQualityParameterIdAsync(id)).ReturnsAsync(2);
            _mockQueryRepo.Setup(r => r.IsUomRequiredForDataTypeAsync(2)).ReturnsAsync(uomRequired);
            _mockUomLookup.Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UOMLookupDto> { new UOMLookupDto { Id = unitId, Code = "N", UOMName = "Newton" } });
        }

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            var command = QualityParameterBuilders.ValidUpdateCommand();
            SetupHappyPath();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Id_ZeroOrNegative_FailsValidation(int id)
        {
            var command = QualityParameterBuilders.ValidUpdateCommand(id: id);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), id)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.ParameterGroupExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.GetDataTypeIdByQualityParameterIdAsync(id)).ReturnsAsync((int?)null);
            _mockQueryRepo.Setup(r => r.IsUomRequiredForDataTypeAsync(It.IsAny<int>())).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("Valid Id is required.");
        }

        [Fact]
        public async Task Id_NotFound_FailsValidation()
        {
            var command = QualityParameterBuilders.ValidUpdateCommand(id: 999);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(999)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), 999)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.ParameterGroupExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.GetDataTypeIdByQualityParameterIdAsync(999)).ReturnsAsync((int?)null);
            _mockQueryRepo.Setup(r => r.IsUomRequiredForDataTypeAsync(It.IsAny<int>())).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("Quality Parameter not found.");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task ParameterName_Empty_FailsValidation(string? name)
        {
            var command = QualityParameterBuilders.ValidUpdateCommand(name: name);
            SetupHappyPath(name: name ?? string.Empty);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ParameterName);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(2)]
        public async Task IsActive_InvalidValue_FailsValidation(int isActive)
        {
            var command = QualityParameterBuilders.ValidUpdateCommand(isActive: isActive);
            SetupHappyPath();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.IsActive)
                  .WithErrorMessage("IsActive  must be either 0 or 1.");
        }

        [Fact]
        public async Task UomRequired_ButMissing_FailsValidation()
        {
            var command = QualityParameterBuilders.ValidUpdateCommand(unitId: null);
            SetupHappyPath(unitId: 0, uomRequired: true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.UnitId)
                  .WithErrorMessage("Unit of Measure is required for Numeric/Decimal data types, and must be empty otherwise.");
        }
    }
}
