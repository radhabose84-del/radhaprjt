using FluentValidation.TestHelper;
using ProductionManagement.Application.Common.Interfaces.ICertificationMaster;
using ProductionManagement.Application.CertificationMaster.Commands.UpdateCertificationMaster;
using ProductionManagement.Presentation.Validation.CertificationMaster;
using ProductionManagement.Presentation.Validation.Common;

using ProductionManagement.UnitTests.TestHelpers;
namespace ProductionManagement.UnitTests.Validators.CertificationMaster
{
    public sealed class UpdateCertificationMasterCommandValidatorTests
    {
        private readonly Mock<ICertificationMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private UpdateCertificationMasterCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        [Fact]
        public async Task Validate_EmptyCertificationName_FailsValidation()
        {
            var result = await CreateValidator().TestValidateAsync(new UpdateCertificationMasterCommand { Id = 1, CertificationName = null });
            result.ShouldHaveValidationErrorFor(x => x.CertificationName);
        }

        [Fact]
        public async Task Validate_IdRequired_WhenZero_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(0)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(new UpdateCertificationMasterCommand { Id = 0, CertificationName = "Test" });
            result.ShouldHaveAnyValidationError();
        }
    }
}
