using FluentValidation.TestHelper;
using ProductionManagement.Application.Common.Interfaces.IPackType;
using ProductionManagement.Application.PackType.Commands.UpdatePackType;
using ProductionManagement.Presentation.Validation.PackType;
using ProductionManagement.Presentation.Validation.Common;

using ProductionManagement.UnitTests.TestHelpers;
namespace ProductionManagement.UnitTests.Validators.PackType
{
    public sealed class UpdatePackTypeCommandValidatorTests
    {
        private readonly Mock<IPackTypeQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private UpdatePackTypeCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        [Fact]
        public async Task Validate_EmptyCommand_HasValidationErrors()
        {
            var result = await CreateValidator().TestValidateAsync(new UpdatePackTypeCommand { Id = 1 });
            result.ShouldHaveAnyValidationError();
        }
    }
}
