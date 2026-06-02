using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Workflow;
using FluentValidation.TestHelper;
using PartyManagement.Application.Common.Interfaces.IPartyMaster;
using PartyManagement.Presentation.Validation.Common;
using PartyManagement.Presentation.Validation.PartyMaster;
using PartyManagement.UnitTests.TestData;

namespace PartyManagement.UnitTests.Validators.PartyMaster
{
    public sealed class CreatePartyMasterCommandValidatorTests
    {
        private readonly Mock<IPartyMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IPartyMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IWorkflowLookup> _mockWorkflowLookup = new(MockBehavior.Loose);
        private readonly Mock<IBankAccountLookup> _mockBankLookup = new(MockBehavior.Loose);
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        // Constructor: (IPartyMasterCommandRepository, MaxLengthProvider, IWorkflowLookup, IPartyMasterQueryRepository, IBankAccountLookup)
        private CreatePartyMasterCommandValidator CreateValidator() =>
            new(_mockCommandRepo.Object, _mockMaxLength.Object, _mockWorkflowLookup.Object, _mockQueryRepo.Object, _mockBankLookup.Object);

        private void SetupHappyPath()
        {
            _mockWorkflowLookup
                .Setup(w => w.IsApproveWorkflowConfigureAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(true);

            _mockCommandRepo
                .Setup(r => r.ExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupHappyPath();
            var command = PartyMasterBuilders.ValidCreateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyPartyName_FailsValidation(string? partyName)
        {
            SetupHappyPath();
            var command = PartyMasterBuilders.ValidCreateCommand(partyName: partyName!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicatePartyName_FailsValidation()
        {
            _mockWorkflowLookup
                .Setup(w => w.IsApproveWorkflowConfigureAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(true);
            _mockCommandRepo
                .Setup(r => r.ExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            var command = PartyMasterBuilders.ValidCreateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
