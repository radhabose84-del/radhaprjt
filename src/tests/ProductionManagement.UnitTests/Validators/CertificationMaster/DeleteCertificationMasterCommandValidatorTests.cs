using FluentValidation.TestHelper;
using ProductionManagement.Application.Common.Interfaces.ICertificationMaster;
using ProductionManagement.Application.CertificationMaster.Commands.DeleteCertificationMaster;
using ProductionManagement.Presentation.Validation.CertificationMaster;

namespace ProductionManagement.UnitTests.Validators.CertificationMaster
{
    public sealed class DeleteCertificationMasterCommandValidatorTests
    {
        private readonly Mock<ICertificationMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private DeleteCertificationMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var result = await CreateValidator().TestValidateAsync(new DeleteCertificationMasterCommand(0));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFoundId_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(999)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(new DeleteCertificationMasterCommand(999));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_ExistingId_PassesValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            var result = await CreateValidator().TestValidateAsync(new DeleteCertificationMasterCommand(1));
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
