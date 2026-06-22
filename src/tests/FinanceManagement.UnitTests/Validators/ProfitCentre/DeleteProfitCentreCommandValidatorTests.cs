using FinanceManagement.Application.Common.Interfaces.IProfitCentre;
using FinanceManagement.Application.ProfitCentre.Commands.DeleteProfitCentre;
using FinanceManagement.Presentation.Validation.ProfitCentre;
using FluentValidation.TestHelper;

namespace FinanceManagement.UnitTests.Validators.ProfitCentre
{
    public sealed class DeleteProfitCentreCommandValidatorTests
    {
        private readonly Mock<IProfitCentreQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private DeleteProfitCentreCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupHappyPath()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(It.IsAny<int>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(It.IsAny<int>())).ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidId_Passes()
        {
            SetupHappyPath();
            var result = await CreateValidator().TestValidateAsync(new DeleteProfitCentreCommand(1));
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_NotFound_Fails()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(It.IsAny<int>())).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(new DeleteProfitCentreCommand(1));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_HasChildren_Fails()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(It.IsAny<int>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(new DeleteProfitCentreCommand(1));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
    }
}
