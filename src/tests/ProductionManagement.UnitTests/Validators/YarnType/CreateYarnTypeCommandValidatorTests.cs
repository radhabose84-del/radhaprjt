using FluentValidation.TestHelper;
using ProductionManagement.Application.Common.Interfaces.IYarnType;
using ProductionManagement.Application.YarnType.Commands.CreateYarnType;
using ProductionManagement.Presentation.Validation.YarnType;
using ProductionManagement.Presentation.Validation.Common;

using ProductionManagement.UnitTests.TestHelpers;
namespace ProductionManagement.UnitTests.Validators.YarnType
{
    public sealed class CreateYarnTypeCommandValidatorTests
    {
        private readonly Mock<IYarnTypeQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private CreateYarnTypeCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        [Fact]
        public async Task Validate_EmptyCommand_HasValidationErrors()
        {
            var result = await CreateValidator().TestValidateAsync(new CreateYarnTypeCommand());
            result.ShouldHaveAnyValidationError();
        }
    }
}
