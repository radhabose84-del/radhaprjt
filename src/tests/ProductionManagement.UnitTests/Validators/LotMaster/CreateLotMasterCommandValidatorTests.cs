using FluentValidation.TestHelper;
using ProductionManagement.Application.Common.Interfaces.ILotMaster;
using ProductionManagement.Application.LotMaster.Commands.CreateLotMaster;
using ProductionManagement.Presentation.Validation.LotMaster;
using ProductionManagement.Presentation.Validation.Common;

using ProductionManagement.UnitTests.TestHelpers;
namespace ProductionManagement.UnitTests.Validators.LotMaster
{
    public sealed class CreateLotMasterCommandValidatorTests
    {
        private readonly Mock<ILotMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private CreateLotMasterCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        [Fact]
        public async Task Validate_EmptyCommand_HasValidationErrors()
        {
            var result = await CreateValidator().TestValidateAsync(new CreateLotMasterCommand());
            result.ShouldHaveAnyValidationError();
        }
    }
}
