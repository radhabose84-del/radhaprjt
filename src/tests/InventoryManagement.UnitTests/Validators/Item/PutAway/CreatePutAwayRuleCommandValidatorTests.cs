using FluentValidation.TestHelper;
using InventoryManagement.Application.Common.Interfaces.IMiscMaster;
using InventoryManagement.Application.Common.Interfaces.Item.PutAway;
using InventoryManagement.Application.Item.PutAway.Commands.CreatePutAwayRule;
using InventoryManagement.Presentation.Validation.Common;
using InventoryManagement.Presentation.Validation.Item.PutAway;

namespace InventoryManagement.UnitTests.Validators.Item.PutAway
{
    public sealed class CreatePutAwayRuleCommandValidatorTests
    {
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });
        private readonly Mock<IPutAwayRuleCommandRepository> _mockRuleRepo = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterQueryRepository> _mockMiscRepo = new(MockBehavior.Loose);

        private CreatePutAwayRuleCommandValidator CreateValidator() =>
            new(_mockMaxLength.Object, _mockRuleRepo.Object, _mockMiscRepo.Object);

        private static CreatePutAwayRuleCommand ValidCommand() => new(new CreatePutAwayRuleRequest
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
        });

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroUnitId_FailsValidation()
        {
            var request = new CreatePutAwayRuleRequest
            {
                UnitId = 0,
                WarehouseId = 1,
                ItemGroupId = 1,
                ItemCategoryId = 1,
                IsActive = 1,
                Strategies = new List<CreatePutAwayStrategyRequest>
                {
                    new CreatePutAwayStrategyRequest { StorageTypeId = 1, PriorityId = 1, TargetId = 1 }
                }
            };

            var result = await CreateValidator().TestValidateAsync(new CreatePutAwayRuleCommand(request));

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroWarehouseId_FailsValidation()
        {
            var request = new CreatePutAwayRuleRequest
            {
                UnitId = 1,
                WarehouseId = 0,
                ItemGroupId = 1,
                ItemCategoryId = 1,
                IsActive = 1,
                Strategies = new List<CreatePutAwayStrategyRequest>
                {
                    new CreatePutAwayStrategyRequest { StorageTypeId = 1, PriorityId = 1, TargetId = 1 }
                }
            };

            var result = await CreateValidator().TestValidateAsync(new CreatePutAwayRuleCommand(request));

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_EmptyStrategies_FailsValidation()
        {
            var request = new CreatePutAwayRuleRequest
            {
                UnitId = 1,
                WarehouseId = 1,
                ItemGroupId = 1,
                ItemCategoryId = 1,
                IsActive = 1,
                Strategies = new List<CreatePutAwayStrategyRequest>()
            };

            var result = await CreateValidator().TestValidateAsync(new CreatePutAwayRuleCommand(request));

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_StrategyWithZeroStorageTypeId_FailsValidation()
        {
            var request = new CreatePutAwayRuleRequest
            {
                UnitId = 1,
                WarehouseId = 1,
                ItemGroupId = 1,
                ItemCategoryId = 1,
                IsActive = 1,
                Strategies = new List<CreatePutAwayStrategyRequest>
                {
                    new CreatePutAwayStrategyRequest { StorageTypeId = 0, PriorityId = 1, TargetId = 1 }
                }
            };

            var result = await CreateValidator().TestValidateAsync(new CreatePutAwayRuleCommand(request));

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicatePriorities_FailsValidation()
        {
            var request = new CreatePutAwayRuleRequest
            {
                UnitId = 1,
                WarehouseId = 1,
                ItemGroupId = 1,
                ItemCategoryId = 1,
                IsActive = 1,
                Strategies = new List<CreatePutAwayStrategyRequest>
                {
                    new CreatePutAwayStrategyRequest { StorageTypeId = 1, PriorityId = 1, TargetId = 1 },
                    new CreatePutAwayStrategyRequest { StorageTypeId = 2, PriorityId = 1, TargetId = 2 }
                }
            };

            var result = await CreateValidator().TestValidateAsync(new CreatePutAwayRuleCommand(request));

            result.Errors.Should().NotBeEmpty();
        }
    }
}
