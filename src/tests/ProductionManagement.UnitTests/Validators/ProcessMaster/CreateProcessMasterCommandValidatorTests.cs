using FluentValidation.TestHelper;
using ProductionManagement.Application.Common.Interfaces.IProcessMaster;
using ProductionManagement.Application.ProcessMaster.Commands.CreateProcessMaster;
using ProductionManagement.Presentation.Validation.ProcessMaster;
using ProductionManagement.Presentation.Validation.Common;

using ProductionManagement.UnitTests.TestHelpers;
namespace ProductionManagement.UnitTests.Validators.ProcessMaster
{
    public sealed class CreateProcessMasterCommandValidatorTests
    {
        private readonly Mock<IProcessMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private CreateProcessMasterCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        [Fact]
        public async Task Validate_EmptyCommand_HasValidationErrors()
        {
            var result = await CreateValidator().TestValidateAsync(new CreateProcessMasterCommand());
            result.ShouldHaveAnyValidationError();
        }
    }
}
