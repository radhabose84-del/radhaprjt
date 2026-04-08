using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IStoTypeMaster;
using SalesManagement.Application.StoTypeMaster.Commands.DeleteStoTypeMaster;
using SalesManagement.Presentation.Validation.StoTypeMaster;

namespace SalesManagement.UnitTests.Validators.StoTypeMaster
{
    public sealed class DeleteStoTypeMasterCommandValidatorTests
    {
        private readonly Mock<IStoTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteStoTypeMasterCommandValidator CreateValidator()
            => new(_mockQueryRepo.Object);

        private void SetupHappyPath(int id = 1)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
        }

        [Fact]
        public async Task ValidId_PassesValidation()
        {
            SetupHappyPath();
            var result = await CreateValidator().TestValidateAsync(new DeleteStoTypeMasterCommand(1));
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task ZeroId_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(0)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(new DeleteStoTypeMasterCommand(0));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task NotFound_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(new DeleteStoTypeMasterCommand(1));
            result.ShouldHaveAnyValidationError();
        }
    }
}
