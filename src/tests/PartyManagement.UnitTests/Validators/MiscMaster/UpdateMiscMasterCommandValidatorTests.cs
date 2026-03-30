using FluentValidation.TestHelper;
using PartyManagement.Application.Common.Interfaces.IMiscMaster;
using PartyManagement.Presentation.Validation.Common;
using PartyManagement.Presentation.Validation.MiscMaster;
using PartyManagement.UnitTests.TestData;

namespace PartyManagement.UnitTests.Validators.MiscMaster
{
    public sealed class UpdateMiscMasterCommandValidatorTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        // MaxLengthProvider.GetMaxLength is not virtual — use a real instance with null dbContext
        private readonly MaxLengthProvider _maxLengthProvider = new(null!);

        // Constructor order: (IQueryRepository, MaxLengthProvider) — repo first
        private UpdateMiscMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object, _maxLengthProvider);

        private void SetupAllAsyncMocks(int id = 1)
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), It.IsAny<int>(), id))
                .ReturnsAsync(false);
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(id))
                .ReturnsAsync(true);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupAllAsyncMocks();
            var command = MiscMasterBuilders.ValidUpdateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyDescription_FailsValidation(string? description)
        {
            var command = MiscMasterBuilders.ValidUpdateCommand(description: description!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
