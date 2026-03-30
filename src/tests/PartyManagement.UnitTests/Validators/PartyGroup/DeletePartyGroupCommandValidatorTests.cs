using FluentValidation.TestHelper;
using PartyManagement.Application.Common.Interfaces.IPartyGroup;
using PartyManagement.Application.PartyGroup.Queries.GetPartyGroupById;
using PartyManagement.Presentation.Validation.PartyGroup;
using PartyManagement.UnitTests.TestData;

namespace PartyManagement.UnitTests.Validators.PartyGroup
{
    public sealed class DeletePartyGroupCommandValidatorTests
    {
        private readonly Mock<IPartyGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private DeletePartyGroupCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(PartyGroupBuilders.ValidByIdDto());
            var command = PartyGroupBuilders.ValidDeleteCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = PartyGroupBuilders.ValidDeleteCommand(id: 0);

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
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((PartyGroupByIdDto?)null);
            var command = PartyGroupBuilders.ValidDeleteCommand(id: 9999);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().BeEmpty();
        }
    }
}
