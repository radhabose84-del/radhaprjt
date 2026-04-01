using FluentValidation.TestHelper;
using ProductionManagement.Application.Common.Interfaces.ICertificationMaster;
using ProductionManagement.Application.CertificationMaster.Commands.CreateCertificationMaster;
using ProductionManagement.Presentation.Validation.CertificationMaster;
using ProductionManagement.Presentation.Validation.Common;

using ProductionManagement.UnitTests.TestHelpers;
namespace ProductionManagement.UnitTests.Validators.CertificationMaster
{
    public sealed class CreateCertificationMasterCommandValidatorTests
    {
        private readonly Mock<ICertificationMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private CreateCertificationMasterCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        [Fact]
        public async Task Validate_EmptyCertificationName_FailsValidation()
        {
            var result = await CreateValidator().TestValidateAsync(new CreateCertificationMasterCommand { CertificationName = null });
            result.ShouldHaveValidationErrorFor(x => x.CertificationName);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesRequiredChecks()
        {
            _mockQueryRepo.Setup(r => r.CertificationNameExistsAsync(It.IsAny<string>(), null)).ReturnsAsync(false);
            var cmd = new CreateCertificationMasterCommand { CertificationName = "ISO9001" };
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldNotHaveValidationErrorFor(x => x.CertificationName);
        }

        [Fact]
        public async Task Validate_DuplicateName_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.CertificationNameExistsAsync("ISO9001", null)).ReturnsAsync(true);
            var cmd = new CreateCertificationMasterCommand { CertificationName = "ISO9001" };
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.CertificationName);
        }
    }
}
