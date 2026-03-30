using FluentValidation.TestHelper;
using PartyManagement.Application.Common.Interfaces.IPartyGroup;
using PartyManagement.Presentation.Validation.Common;
using PartyManagement.Presentation.Validation.PartyGroup;
using PartyManagement.UnitTests.TestData;

namespace PartyManagement.UnitTests.Validators.PartyGroup
{
    public sealed class CreatePartyGroupCommandValidatorTests
    {
        private readonly Mock<IPartyGroupCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        // Constructor: (IPartyGroupCommandRepository, MaxLengthProvider)
        private CreatePartyGroupCommandValidator CreateValidator() =>
            new(_mockCommandRepo.Object, _mockMaxLength.Object);

        private void SetupNoDuplicate(string name = "Test Party Group", int groupTypeId = 1)
        {
            _mockCommandRepo
                .Setup(r => r.ExistsAsync(name, groupTypeId))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupNoDuplicate();
            var command = PartyGroupBuilders.ValidCreateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyPartyGroupName_FailsValidation(string? name)
        {
            var command = PartyGroupBuilders.ValidCreateCommand(name: name!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicateNameAndGroupType_FailsValidation()
        {
            _mockCommandRepo
                .Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(true);
            var command = PartyGroupBuilders.ValidCreateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
