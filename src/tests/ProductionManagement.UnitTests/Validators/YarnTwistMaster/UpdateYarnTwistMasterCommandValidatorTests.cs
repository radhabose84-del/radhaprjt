using FluentValidation.TestHelper;
using ProductionManagement.Application.Common.Interfaces.IYarnTwistMaster;
using ProductionManagement.Application.YarnTwistMaster.Commands.UpdateYarnTwistMaster;
using ProductionManagement.Presentation.Validation.YarnTwistMaster;
using ProductionManagement.Presentation.Validation.Common;

using ProductionManagement.UnitTests.TestHelpers;
namespace ProductionManagement.UnitTests.Validators.YarnTwistMaster
{
    public sealed class UpdateYarnTwistMasterCommandValidatorTests
    {
        private readonly Mock<IYarnTwistMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private UpdateYarnTwistMasterCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        [Fact]
        public async Task Validate_EmptyCommand_HasValidationErrors()
        {
            var result = await CreateValidator().TestValidateAsync(new UpdateYarnTwistMasterCommand { Id = 1 });
            result.ShouldHaveAnyValidationError();
        }
    }
}
