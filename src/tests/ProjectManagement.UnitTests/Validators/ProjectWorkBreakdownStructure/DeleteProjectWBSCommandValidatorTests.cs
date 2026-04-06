using FluentValidation.TestHelper;
using ProjectManagement.Application.Common.Interfaces.IProjectWorkBreakdownStructure;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Command.SoftDeleteProjectWorkBreakdownStructureCommand;
using ProjectManagement.Presentation.Validation.ProjectWorkBreakdownStructure;

namespace ProjectManagement.UnitTests.Validators.ProjectWorkBreakdownStructure
{
    public sealed class DeleteProjectWBSCommandValidatorTests
    {
        private readonly Mock<IProjectWorkBreakdownStructureQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private DeleteProjectWorkBreakdownStructureCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ValidId_PassesValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(1)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(new DeleteProjectWorkBreakdownStructureCommand(1));
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(0)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(0)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(new DeleteProjectWorkBreakdownStructureCommand(0));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NegativeId_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(-1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(-1)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(new DeleteProjectWorkBreakdownStructureCommand(-1));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
    }
}
