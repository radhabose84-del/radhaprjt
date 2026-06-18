using FinanceManagement.Application.Common.Interfaces.ICostCentre;
using FinanceManagement.Application.CostCentre.Commands.UpdateCostCentre;
using FinanceManagement.Presentation.Validation.CostCentre;
using FinanceManagement.UnitTests.TestHelpers;
using FluentValidation.TestHelper;

namespace FinanceManagement.UnitTests.Validators.CostCentre
{
    public sealed class UpdateCostCentreCommandValidatorTests
    {
        private readonly Mock<ICostCentreQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private UpdateCostCentreCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        private void SetupHappyPath()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(It.IsAny<int>())).ReturnsAsync(false);
        }

        private static UpdateCostCentreCommand ValidCommand() =>
            new() { Id = 1, CostCentreName = "Plant", IsActive = 1 };

        [Fact]
        public async Task Validate_ValidCommand_Passes()
        {
            SetupHappyPath();
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyName_Fails(string? name)
        {
            SetupHappyPath();
            var cmd = ValidCommand();
            cmd.CostCentreName = name;

            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.CostCentreName);
        }

        [Fact]
        public async Task Validate_NotFound_Fails()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(-1)]
        public async Task Validate_IsActiveOutOfRange_Fails(int isActive)
        {
            SetupHappyPath();
            var cmd = ValidCommand();
            cmd.IsActive = isActive;

            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.IsActive);
        }
    }
}
