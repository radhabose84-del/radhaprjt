using FluentValidation.TestHelper;
using InventoryManagement.Application.Common.Interfaces.Item.PutAway;
using InventoryManagement.Application.Item.PutAway.Commands.DeletePutAwayRule;
using InventoryManagement.Presentation.Validation.Item.PutAway;

namespace InventoryManagement.UnitTests.Validators.Item.PutAway
{
    public sealed class DeletePutAwayRuleCommandValidatorTests
    {
        private readonly Mock<IPutAwayRuleCommandRepository> _mockRuleRepo = new(MockBehavior.Loose);
        private readonly Mock<IPutAwayRuleQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        public DeletePutAwayRuleCommandValidatorTests()
        {
            _mockRuleRepo
                .Setup(r => r.ExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidationAsync(It.IsAny<int>()))
                .ReturnsAsync(false);
        }

        private DeletePutAwayRuleCommandValidator CreateValidator() => new(_mockRuleRepo.Object, _mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ValidId_PassesValidation()
        {
            var command = new DeletePutAwayRuleCommand { Id = 1 };
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = new DeletePutAwayRuleCommand { Id = 0 };
            var result = await CreateValidator().TestValidateAsync(command);
            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_RuleNotFound_FailsValidation()
        {
            _mockRuleRepo
                .Setup(r => r.ExistsAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var command = new DeletePutAwayRuleCommand { Id = 99 };
            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
