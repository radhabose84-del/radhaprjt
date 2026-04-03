using FluentValidation.TestHelper;
using InventoryManagement.Application.Budget.Commands.CreateBudget;
using InventoryManagement.Application.Common.Interfaces.Budget;
using InventoryManagement.Presentation.Validation.Budget;

namespace InventoryManagement.UnitTests.Validators.Budget
{
    public sealed class CreateBudgetCommandValidatorTests
    {
        private readonly Mock<IBudgetCommandRepository> _mockBudgetRepo = new(MockBehavior.Loose);

        public CreateBudgetCommandValidatorTests()
        {
            _mockBudgetRepo
                .Setup(r => r.ExistsAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(false);
        }

        private CreateBudgetCommandValidator CreateValidator() => new(_mockBudgetRepo.Object);

        private static CreateBudgetCommand ValidCommand() => new()
        {
            BudgetGroupId = 1,
            FiscalYear = 2024,
            YearBudgetAmount = 100000m,
            Is_MRApplicable = 1,
            Is_POApplicable = 0,
            Is_ServiceApplicable = 0,
            BudgetDetails = new List<BudgetDetailDto>()
        };

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroBudgetGroupId_FailsValidation()
        {
            var command = ValidCommand();
            command.BudgetGroupId = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroFiscalYear_FailsValidation()
        {
            var command = ValidCommand();
            command.FiscalYear = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroYearBudgetAmount_FailsValidation()
        {
            var command = ValidCommand();
            command.YearBudgetAmount = 0m;

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicateBudget_FailsValidation()
        {
            _mockBudgetRepo
                .Setup(r => r.ExistsAsync(1, 2024))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(ValidCommand());

            result.Errors.Should().NotBeEmpty();
        }
    }
}
