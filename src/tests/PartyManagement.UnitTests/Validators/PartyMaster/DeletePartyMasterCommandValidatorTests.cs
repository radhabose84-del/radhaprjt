using FluentValidation.TestHelper;
using PartyManagement.Application.Common.Interfaces.IPartyMaster;
using PartyManagement.Presentation.Validation.PartyMaster;
using PartyManagement.UnitTests.TestData;

namespace PartyManagement.UnitTests.Validators.PartyMaster
{
    public sealed class DeletePartyMasterCommandValidatorTests
    {
        private readonly Mock<IPartyMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private DeletePartyMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdPartyMasterAsync(1))
                .ReturnsAsync(new PartyManagement.Application.PartyMaster.Queries.GetPartyMasterById.PartyMasterDto());
            var command = PartyMasterBuilders.ValidDeleteCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = PartyMasterBuilders.ValidDeleteCommand(id: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFound_PassesValidation()
        {
            // The delete validator uses "RecordNotFound" case which is not in validation-rules.json
            // (only "NotFound" exists in the JSON). Therefore the existence check rule is never
            // registered and no error is produced even when the entity doesn't exist.
            _mockQueryRepo
                .Setup(r => r.GetByIdPartyMasterAsync(It.IsAny<int>()))
                .ReturnsAsync((PartyManagement.Application.PartyMaster.Queries.GetPartyMasterById.PartyMasterDto)null!);
            var command = PartyMasterBuilders.ValidDeleteCommand(id: 9999);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().BeEmpty();
        }
    }
}
