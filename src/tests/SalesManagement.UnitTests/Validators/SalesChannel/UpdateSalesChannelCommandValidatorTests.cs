using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.ISalesChannel;
using SalesManagement.Application.SalesChannel.Commands.UpdateSalesChannel;
using SalesManagement.Presentation.Validation.SalesChannel;
using SalesManagement.UnitTests.TestData;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.SalesChannel
{
    public class UpdateSalesChannelCommandValidatorTests
    {
        private readonly Mock<ISalesChannelQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private UpdateSalesChannelCommandValidator CreateValidator()
            => new UpdateSalesChannelCommandValidator(
                TestMaxLengthProviderFactory.Create(),
                _mockQueryRepo.Object);

        // ── Setup helpers ─────────────────────────────────────────────────────

        private void SetupIdExists(int id = 1)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
        }

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            SetupIdExists(1);
            var command = SalesChannelBuilders.ValidUpdateCommand(id: 1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        // ── Id Rules ─────────────────────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Id_ZeroOrNegative_FailsValidation(int id)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(true);
            var command = SalesChannelBuilders.ValidUpdateCommand(id: id);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("Valid Id is required.");
        }

        // ── SalesChannelName Rules ────────────────────────────────────────────

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task SalesChannelName_Empty_FailsValidation(string? name)
        {
            SetupIdExists(1);
            var command = SalesChannelBuilders.ValidUpdateCommand(id: 1, name: name);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SalesChannelName)
                  .WithErrorMessage("SalesChannelName is required.");
        }

        [Fact]
        public async Task SalesChannelName_TooLong_FailsValidation()
        {
            SetupIdExists(1);
            var longName = new string('A', 101);
            var command = SalesChannelBuilders.ValidUpdateCommand(id: 1, name: longName);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SalesChannelName)
                  .WithErrorMessage("SalesChannelName  cannot be longer than   100 characters.");
        }

        [Fact]
        public async Task SalesChannelName_MaxLength100_PassesValidation()
        {
            SetupIdExists(1);
            var maxName = new string('A', 100);
            var command = SalesChannelBuilders.ValidUpdateCommand(id: 1, name: maxName);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.SalesChannelName);
        }

        // ── Immutability Verification ─────────────────────────────────────────

        [Fact]
        public void UpdateCommand_DoesNotContain_SalesChannelCode_Property()
        {
            // SalesChannelCode is immutable and must NOT be in the Update command
            var commandType = typeof(UpdateSalesChannelCommand);
            var codeProperty = commandType.GetProperty("SalesChannelCode");

            codeProperty.Should().BeNull(
                "SalesChannelCode is immutable and must NOT be included in UpdateSalesChannelCommand");
        }
    }
}
