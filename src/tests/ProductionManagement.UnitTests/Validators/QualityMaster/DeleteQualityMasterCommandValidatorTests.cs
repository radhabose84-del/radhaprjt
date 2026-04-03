using FluentValidation.TestHelper;
using ProductionManagement.Application.Common.Interfaces.IQualityMaster;
using ProductionManagement.Application.QualityMaster.Commands.DeleteQualityMaster;
using ProductionManagement.Presentation.Validation.QualityMaster;

namespace ProductionManagement.UnitTests.Validators.QualityMaster
{
    public sealed class DeleteQualityMasterCommandValidatorTests
    {
        private readonly Mock<IQualityMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private DeleteQualityMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var result = await CreateValidator().TestValidateAsync(new DeleteQualityMasterCommand(0));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFoundId_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(999)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(new DeleteQualityMasterCommand(999));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_ExistingId_PassesValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            var result = await CreateValidator().TestValidateAsync(new DeleteQualityMasterCommand(1));
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
