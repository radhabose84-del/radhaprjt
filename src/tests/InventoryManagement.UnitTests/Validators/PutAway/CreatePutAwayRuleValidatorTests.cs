using FluentValidation.TestHelper;
using InventoryManagement.Application.Common.Interfaces.IMiscMaster;
using InventoryManagement.Application.Common.Interfaces.Item.PutAway;
using InventoryManagement.Application.Item.PutAway.Commands.CreatePutAwayRule;
using InventoryManagement.Presentation.Validation.Common;
using InventoryManagement.Presentation.Validation.Item.PutAway;

namespace InventoryManagement.UnitTests.Validators.PutAway
{
    public sealed class CreatePutAwayRuleValidatorTests
    {
        private readonly Mock<IPutAwayRuleCommandRepository> _mockRuleRepo = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterQueryRepository> _mockMiscRepo = new(MockBehavior.Loose);

        private CreatePutAwayRuleCommandValidator CreateValidator() =>
            new(new MaxLengthProvider(null!), _mockRuleRepo.Object, _mockMiscRepo.Object);

        [Fact]
        public async Task Validate_EmptyBody_FailsValidation()
        {
            var command = new CreatePutAwayRuleCommand(new CreatePutAwayRuleRequest());

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveAnyValidationError();
        }
    }
}
