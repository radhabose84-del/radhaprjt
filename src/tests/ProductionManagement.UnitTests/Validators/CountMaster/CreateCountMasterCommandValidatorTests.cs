using FluentValidation.TestHelper;
using Contracts.Interfaces.Lookups.Inventory;
using ProductionManagement.Application.Common.Interfaces.ICountMaster;
using ProductionManagement.Application.CountMaster.Commands.CreateCountMaster;
using ProductionManagement.Presentation.Validation.CountMaster;
using ProductionManagement.Presentation.Validation.Common;

using ProductionManagement.UnitTests.TestHelpers;
namespace ProductionManagement.UnitTests.Validators.CountMaster
{
    public sealed class CreateCountMasterCommandValidatorTests
    {
        private readonly Mock<ICountMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IUOMLookup> _mockUomLookup = new(MockBehavior.Loose);

        private CreateCountMasterCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object, _mockUomLookup.Object);

        [Fact]
        public async Task Validate_EmptyCommand_HasValidationErrors()
        {
            var result = await CreateValidator().TestValidateAsync(new CreateCountMasterCommand());
            result.ShouldHaveAnyValidationError();
        }
    }
}
