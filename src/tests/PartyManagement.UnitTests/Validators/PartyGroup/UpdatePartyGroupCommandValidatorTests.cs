using FluentValidation.TestHelper;
using PartyManagement.Application.Common.Interfaces.IPartyGroup;
using PartyManagement.Presentation.Validation.Common;
using PartyManagement.Presentation.Validation.PartyGroup;
using PartyManagement.UnitTests.TestData;

namespace PartyManagement.UnitTests.Validators.PartyGroup
{
    public sealed class UpdatePartyGroupCommandValidatorTests
    {
        private readonly Mock<IPartyGroupCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        // Constructor: (IPartyGroupCommandRepository, MaxLengthProvider)
        private UpdatePartyGroupCommandValidator CreateValidator() =>
            new(_mockCommandRepo.Object, _mockMaxLength.Object);

        private void SetupForUpdate(int id = 1, string name = "Updated Party Group", int groupTypeId = 1)
        {
            _mockCommandRepo
                .Setup(r => r.GetGroupTypeIdByIdAsync(id))
                .ReturnsAsync(groupTypeId);

            _mockCommandRepo
                .Setup(r => r.ExistsUpdateAsync(name, groupTypeId, id))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupForUpdate();
            var command = PartyGroupBuilders.ValidUpdateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyPartyGroupName_FailsValidation(string? name)
        {
            var command = PartyGroupBuilders.ValidUpdateCommand(name: name!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicateName_FailsValidation()
        {
            _mockCommandRepo
                .Setup(r => r.GetGroupTypeIdByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(1);
            _mockCommandRepo
                .Setup(r => r.ExistsUpdateAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int?>()))
                .ReturnsAsync(true);
            var command = PartyGroupBuilders.ValidUpdateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
