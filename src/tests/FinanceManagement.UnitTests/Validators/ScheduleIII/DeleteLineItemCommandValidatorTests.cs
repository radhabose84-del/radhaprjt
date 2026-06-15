using FluentValidation.TestHelper;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Commands.DeleteLineItem;
using FinanceManagement.Presentation.Validation.ScheduleIII;

namespace FinanceManagement.UnitTests.Validators.ScheduleIII
{
    public sealed class DeleteLineItemCommandValidatorTests
    {
        private readonly Mock<IScheduleIIIQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private DeleteLineItemCommandValidator CreateValidator() => new(_mockQueryRepo.Object);

        private void SetupHappyPath(int id = 14)
        {
            _mockQueryRepo.Setup(r => r.LineItemNotFoundAsync(id)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.IsLineMappedAsync(id)).ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_UnmappedExistingLine_Passes()
        {
            SetupHappyPath();
            var result = await CreateValidator().TestValidateAsync(new DeleteLineItemCommand(14));
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_Fails()
        {
            var result = await CreateValidator().TestValidateAsync(new DeleteLineItemCommand(0));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFound_Fails()
        {
            SetupHappyPath();
            _mockQueryRepo.Setup(r => r.LineItemNotFoundAsync(14)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(new DeleteLineItemCommand(14));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_MappedLine_IsBlocked_AC5()
        {
            SetupHappyPath();
            _mockQueryRepo.Setup(r => r.IsLineMappedAsync(14)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(new DeleteLineItemCommand(14));

            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("Cannot delete — account group(s) are mapped to this line. Re-map them first.");
        }
    }
}
