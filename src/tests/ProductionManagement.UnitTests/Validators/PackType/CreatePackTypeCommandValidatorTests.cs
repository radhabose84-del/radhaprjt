using FluentValidation.TestHelper;
using ProductionManagement.Application.Common.Interfaces.IPackType;
using ProductionManagement.Application.PackType.Commands.CreatePackType;
using ProductionManagement.Presentation.Validation.PackType;
using ProductionManagement.Presentation.Validation.Common;

using ProductionManagement.UnitTests.TestHelpers;
namespace ProductionManagement.UnitTests.Validators.PackType
{
    public sealed class CreatePackTypeCommandValidatorTests
    {
        private readonly Mock<IPackTypeQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private CreatePackTypeCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        [Fact]
        public async Task Validate_EmptyCommand_HasValidationErrors()
        {
            var result = await CreateValidator().TestValidateAsync(new CreatePackTypeCommand());
            result.ShouldHaveAnyValidationError();
        }
    }
}
