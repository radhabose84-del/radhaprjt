using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IDiscountMaster;
using SalesManagement.Application.DiscountMaster.Commands.DeleteDiscountMaster;
using SalesManagement.Presentation.Validation.DiscountMaster;

namespace SalesManagement.UnitTests.Validators.DiscountMaster
{
    public class DeleteDiscountMasterCommandValidatorTests
    {
        private readonly Mock<IDiscountMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteDiscountMasterCommandValidator CreateValidator()
            => new(_mockQueryRepo.Object);

        private void SetupAllValid()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(It.IsAny<int>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(It.IsAny<int>())).ReturnsAsync(false);
        }

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            SetupAllValid();

            var result = await CreateValidator().TestValidateAsync(new DeleteDiscountMasterCommand(1));

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Id_Zero_FailsValidation()
        {
            SetupAllValid();

            var result = await CreateValidator().TestValidateAsync(new DeleteDiscountMasterCommand(0));

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task NotFound_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(99)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(new DeleteDiscountMasterCommand(99));

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
    }
}
