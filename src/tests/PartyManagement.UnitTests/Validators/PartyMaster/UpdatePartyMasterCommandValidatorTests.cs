using Contracts.Interfaces.Lookups.Party;
using FluentValidation.TestHelper;
using PartyManagement.Application.Common.Interfaces.IPartyMaster;
using PartyManagement.Application.PartyMaster.Command.UpdatePartyMaster;
using PartyManagement.Presentation.Validation.PartyMaster;
using PartyManagement.UnitTests.TestData;

namespace PartyManagement.UnitTests.Validators.PartyMaster
{
    public sealed class UpdatePartyMasterCommandValidatorTests
    {
        private readonly Mock<IPartyMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IPartyMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IBankAccountLookup> _mockBankLookup = new(MockBehavior.Loose);

        // Constructor: (IPartyMasterCommandRepository, IPartyMasterQueryRepository, IBankAccountLookup)
        private UpdatePartyMasterCommandValidator CreateValidator() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockBankLookup.Object);

        private void SetupNoDuplicate(string partyName = "Updated Party", int id = 1)
        {
            _mockCommandRepo
                .Setup(r => r.ExistsForUpdateAsync(partyName, id))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupNoDuplicate();
            var command = PartyMasterBuilders.ValidUpdateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyPartyName_FailsValidation(string? partyName)
        {
            var command = new UpdatePartyMasterCommand
            {
                UpdatePartyMaster = new UpdatePartyMasterDto { Id = 1, PartyName = partyName }
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicatePartyName_FailsValidation()
        {
            _mockCommandRepo
                .Setup(r => r.ExistsForUpdateAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(true);
            var command = PartyMasterBuilders.ValidUpdateCommand();

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

        private static void WithPartyTypes(UpdatePartyMasterCommand command, params int[] partyTypeIds)
        {
            command.UpdatePartyMaster!.PartyTypesUpdate = partyTypeIds
                .Select(id => new UpdatePartyMasterDto.UpdatePartyTypeDto { PartyTypeId = id, PartyGroupId = 1 })
                .ToList();
        }

        [Fact]
        public async Task Validate_AgentAndBroker_FailsValidation()
        {
            SetupNoDuplicate();
            SetupPartyTypeCodes("AGENT", "BROKER");
            var command = PartyMasterBuilders.ValidUpdateCommand();
            WithPartyTypes(command, 3, 79);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().Contain(e => e.ErrorMessage == "A party cannot be both Agent and Broker.");
        }

        [Fact]
        public async Task Validate_SupplierAndGinner_FailsValidation()
        {
            SetupNoDuplicate();
            SetupPartyTypeCodes("SUPPLIER", "GINNER");
            var command = PartyMasterBuilders.ValidUpdateCommand();
            WithPartyTypes(command, 1, 80);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().Contain(e => e.ErrorMessage == "A party cannot be both Supplier and Ginner.");
        }

        [Fact]
        public async Task Validate_BrokerOnly_PassesExclusivity()
        {
            SetupNoDuplicate();
            SetupPartyTypeCodes("BROKER");
            var command = PartyMasterBuilders.ValidUpdateCommand();
            WithPartyTypes(command, 79);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotContain(e => e.ErrorMessage == "A party cannot be both Agent and Broker.");
            result.Errors.Should().NotContain(e => e.ErrorMessage == "A party cannot be both Supplier and Ginner.");
        }
    }
}
