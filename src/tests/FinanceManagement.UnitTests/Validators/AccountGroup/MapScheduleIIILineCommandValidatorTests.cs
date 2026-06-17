using FluentValidation.TestHelper;
using FinanceManagement.Application.AccountGroup.Commands.MapScheduleIIILine;
using FinanceManagement.Application.Common.Interfaces.IAccountGroup;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Presentation.Validation.AccountGroup;

namespace FinanceManagement.UnitTests.Validators.AccountGroup
{
    public sealed class MapScheduleIIILineCommandValidatorTests
    {
        private readonly Mock<IAccountGroupQueryRepository> _mockAccountGroupRepo = new(MockBehavior.Loose);
        private readonly Mock<IScheduleIIIQueryRepository> _mockScheduleIIIRepo = new(MockBehavior.Loose);

        private MapScheduleIIILineCommandValidator CreateValidator() =>
            new(_mockAccountGroupRepo.Object, _mockScheduleIIIRepo.Object);

        [Fact]
        public async Task Validate_ValidMapping_Passes()
        {
            _mockAccountGroupRepo.Setup(r => r.NotFoundAsync(3)).ReturnsAsync(false);
            _mockScheduleIIIRepo.Setup(r => r.LineItemNotFoundAsync(10)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(
                new MapScheduleIIILineCommand { AccountGroupId = 3, ScheduleIIILineItemId = 10 });

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_Unmap_PassesWithoutLineCheck()
        {
            _mockAccountGroupRepo.Setup(r => r.NotFoundAsync(3)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(
                new MapScheduleIIILineCommand { AccountGroupId = 3, ScheduleIIILineItemId = null });

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_AccountGroupNotFound_Fails()
        {
            _mockAccountGroupRepo.Setup(r => r.NotFoundAsync(3)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(
                new MapScheduleIIILineCommand { AccountGroupId = 3, ScheduleIIILineItemId = 10 });

            result.ShouldHaveValidationErrorFor(x => x.AccountGroupId);
        }

        [Fact]
        public async Task Validate_LineItemNotFound_Fails()
        {
            _mockAccountGroupRepo.Setup(r => r.NotFoundAsync(3)).ReturnsAsync(false);
            _mockScheduleIIIRepo.Setup(r => r.LineItemNotFoundAsync(99)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(
                new MapScheduleIIILineCommand { AccountGroupId = 3, ScheduleIIILineItemId = 99 });

            result.ShouldHaveValidationErrorFor(x => x.ScheduleIIILineItemId);
        }
    }
}
