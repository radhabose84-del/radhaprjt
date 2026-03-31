using FluentValidation.TestHelper;
using PartyManagement.Application.Common.Interfaces.IMiscMaster;
using PartyManagement.Presentation.Validation.MiscMaster;
using PartyManagement.UnitTests.TestData;

namespace PartyManagement.UnitTests.Validators.MiscMaster
{
    public sealed class DeleteMiscMasterCommandValidatorTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private DeleteMiscMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ValidId_PassesValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(true);
            var command = MiscMasterBuilders.ValidDeleteCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = new PartyManagement.Application.MiscMaster.Command.DeleteMiscMaster.DeleteMiscMasterCommand { Id = 0 };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
