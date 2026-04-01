using FluentValidation.TestHelper;
using ProductionManagement.Application.Common.Interfaces.IQualityMaster;
using ProductionManagement.Application.QualityMaster.Commands.CreateQualityMaster;
using ProductionManagement.Presentation.Validation.QualityMaster;
using ProductionManagement.Presentation.Validation.Common;

using ProductionManagement.UnitTests.TestHelpers;
namespace ProductionManagement.UnitTests.Validators.QualityMaster
{
    public sealed class CreateQualityMasterCommandValidatorTests
    {
        private readonly Mock<IQualityMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private CreateQualityMasterCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        [Fact]
        public async Task Validate_EmptyCommand_HasValidationErrors()
        {
            var result = await CreateValidator().TestValidateAsync(new CreateQualityMasterCommand());
            result.ShouldHaveAnyValidationError();
        }
    }
}
