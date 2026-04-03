using FluentValidation.TestHelper;
using ProductionManagement.Application.Common.Interfaces.ICountGroup;
using ProductionManagement.Application.CountGroup.Commands.UpdateCountGroup;
using ProductionManagement.Presentation.Validation.CountGroup;
using ProductionManagement.Presentation.Validation.Common;

using ProductionManagement.UnitTests.TestHelpers;
namespace ProductionManagement.UnitTests.Validators.CountGroup
{
    public sealed class UpdateCountGroupCommandValidatorTests
    {
        private readonly Mock<ICountGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private UpdateCountGroupCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        [Fact]
        public async Task Validate_EmptyCommand_HasValidationErrors()
        {
            var result = await CreateValidator().TestValidateAsync(new UpdateCountGroupCommand { Id = 1 });
            result.ShouldHaveAnyValidationError();
        }
    }
}
