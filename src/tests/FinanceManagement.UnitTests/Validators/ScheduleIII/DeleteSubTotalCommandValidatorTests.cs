using FluentValidation.TestHelper;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Commands.DeleteSubTotal;
using FinanceManagement.Presentation.Validation.ScheduleIII;

namespace FinanceManagement.UnitTests.Validators.ScheduleIII
{
    public sealed class DeleteSubTotalCommandValidatorTests
    {
        private readonly Mock<IScheduleIIIQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private DeleteSubTotalCommandValidator CreateValidator() => new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ExistingId_Passes()
        {
            _mockQueryRepo.Setup(r => r.SubTotalNotFoundAsync(3)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(new DeleteSubTotalCommand(3));

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_NonExistentId_Fails()
        {
            _mockQueryRepo.Setup(r => r.SubTotalNotFoundAsync(99)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(new DeleteSubTotalCommand(99));

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
    }
}
