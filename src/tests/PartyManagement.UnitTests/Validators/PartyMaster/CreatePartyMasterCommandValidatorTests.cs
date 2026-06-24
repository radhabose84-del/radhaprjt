using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Workflow;
using FluentValidation.TestHelper;
using PartyManagement.Application.Common.Interfaces.IPartyMaster;
using PartyManagement.Application.PartyMaster.Command.CreatePartyMaster;
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

        // ------------------- Party-type mutual exclusivity -------------------

        private void SetupPartyTypeCodes(params string[] codes)
        {
            _mockQueryRepo
                .Setup(r => r.GetPartyTypeCodesByIdsAsync(It.IsAny<IReadOnlyList<int>>()))
                .ReturnsAsync(codes.ToList());
        }

        private static void WithPartyTypes(CreatePartyMasterCommand command, params int[] partyTypeIds)
        {
            command.PartyMaster.PartyTypes = partyTypeIds
                .Select(id => new CreatePartyMasterDto.PartyTypeDto { PartyTypeId = id, PartyGroupId = 1 })
                .ToList();
        }

        [Fact]
        public async Task Validate_AgentAndBroker_FailsValidation()
        {
            SetupHappyPath();
            SetupPartyTypeCodes("AGENT", "BROKER");
            var command = PartyMasterBuilders.ValidCreateCommand();
            WithPartyTypes(command, 3, 79);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().Contain(e => e.ErrorMessage == "A party cannot be both Agent and Broker.");
        }

        [Fact]
        public async Task Validate_SupplierAndGinner_FailsValidation()
        {
            SetupHappyPath();
            SetupPartyTypeCodes("SUPPLIER", "GINNER");
            var command = PartyMasterBuilders.ValidCreateCommand();
            WithPartyTypes(command, 1, 80);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().Contain(e => e.ErrorMessage == "A party cannot be both Supplier and Ginner.");
        }

        [Fact]
        public async Task Validate_BrokerOnly_PassesExclusivity()
        {
            SetupHappyPath();
            SetupPartyTypeCodes("BROKER");
            var command = PartyMasterBuilders.ValidCreateCommand();
            WithPartyTypes(command, 79);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotContain(e => e.ErrorMessage == "A party cannot be both Agent and Broker.");
            result.Errors.Should().NotContain(e => e.ErrorMessage == "A party cannot be both Supplier and Ginner.");
        }
    }
}
