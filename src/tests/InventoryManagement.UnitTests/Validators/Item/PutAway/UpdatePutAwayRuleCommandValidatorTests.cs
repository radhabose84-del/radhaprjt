using FluentValidation.TestHelper;
using InventoryManagement.Application.Common.Interfaces.IMiscMaster;
using InventoryManagement.Application.Common.Interfaces.Item.PutAway;
using InventoryManagement.Application.Item.PutAway.Commands.CreatePutAwayRule;
using InventoryManagement.Application.Item.PutAway.Commands.UpdatePutAwayRule;
using InventoryManagement.Presentation.Validation.Common;
using InventoryManagement.Presentation.Validation.Item.PutAway;

namespace InventoryManagement.UnitTests.Validators.Item.PutAway
{
    public sealed class UpdatePutAwayRuleCommandValidatorTests
    {
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });
        private readonly Mock<IPutAwayRuleCommandRepository> _mockRuleRepo = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterQueryRepository> _mockMiscRepo = new(MockBehavior.Loose);

        public UpdatePutAwayRuleCommandValidatorTests()
        {
            _mockRuleRepo
                .Setup(r => r.ExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
        }

        private UpdatePutAwayRuleCommandValidator CreateValidator() =>
            new(_mockMaxLength.Object, _mockRuleRepo.Object, _mockMiscRepo.Object);

        private static CreatePutAwayRuleRequest ValidBody() => new()
        {
            UnitId = 1,
            WarehouseId = 1,
            ItemGroupId = 1,
            ItemCategoryId = 1,
            IsActive = 1,
            Strategies = new List<CreatePutAwayStrategyRequest>
            {
                new CreatePutAwayStrategyRequest { StorageTypeId = 1, PriorityId = 1, TargetId = 1 }
            }
        };

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var result = await CreateValidator().TestValidateAsync(new UpdatePutAwayRuleCommand(1, ValidBody()));
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var result = await CreateValidator().TestValidateAsync(new UpdatePutAwayRuleCommand(0, ValidBody()));
            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_RuleNotFound_FailsValidation()
        {
            _mockRuleRepo
                .Setup(r => r.ExistsAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(new UpdatePutAwayRuleCommand(99, ValidBody()));

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_EmptyStrategiesInBody_FailsValidation()
        {
            var body = ValidBody();
            body.Strategies = new List<CreatePutAwayStrategyRequest>();

            var result = await CreateValidator().TestValidateAsync(new UpdatePutAwayRuleCommand(1, body));

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicatePrioritiesInBody_FailsValidation()
        {
            var body = ValidBody();
            body.Strategies = new List<CreatePutAwayStrategyRequest>
            {
                new CreatePutAwayStrategyRequest { StorageTypeId = 1, PriorityId = 1, TargetId = 1 },
                new CreatePutAwayStrategyRequest { StorageTypeId = 2, PriorityId = 1, TargetId = 2 }
            };

            var result = await CreateValidator().TestValidateAsync(new UpdatePutAwayRuleCommand(1, body));

            result.Errors.Should().NotBeEmpty();
        }
    }
}
