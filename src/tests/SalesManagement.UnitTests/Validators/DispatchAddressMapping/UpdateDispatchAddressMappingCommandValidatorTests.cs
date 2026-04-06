using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IDispatchAddressMapping;
using SalesManagement.Application.DispatchAddressMapping.Commands.UpdateDispatchAddressMapping;
using SalesManagement.Application.DispatchAddressMapping.Dto;
using SalesManagement.Presentation.Validation.DispatchAddressMapping;

namespace SalesManagement.UnitTests.Validators.DispatchAddressMapping
{
    public sealed class UpdateDispatchAddressMappingCommandValidatorTests
    {
        private readonly Mock<IDispatchAddressMappingQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private UpdateDispatchAddressMappingCommandValidator CreateValidator()
            => new(_mockQueryRepo.Object);

        private void SetupAllAsyncMocks(int id = 1)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(new DispatchAddressMappingDto { Id = id, PartyId = 1, UsageTypeId = 1 });
            _mockQueryRepo.Setup(r => r.DefaultAlreadyExistsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>())).ReturnsAsync(false);
        }

        private static UpdateDispatchAddressMappingCommand ValidCommand() => new()
        {
            Id = 1,
            IsDefault = false,
            IsActive = 1
        };

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(ValidCommand());

            result.ShouldNotHaveAnyValidationErrors();
        }

        // ── Id Rules ─────────────────────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Id_ZeroOrNegative_FailsValidation(int id)
        {
            var cmd = ValidCommand();
            cmd.Id = id;
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((DispatchAddressMappingDto?)null);
            _mockQueryRepo.Setup(r => r.DefaultAlreadyExistsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>())).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Id_NotFound_FailsValidation()
        {
            var cmd = ValidCommand();
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((DispatchAddressMappingDto?)null);
            _mockQueryRepo.Setup(r => r.DefaultAlreadyExistsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>())).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        // ── IsActive Rules ────────────────────────────────────────────────────

        [Theory]
        [InlineData(2)]
        [InlineData(-1)]
        public async Task IsActive_InvalidValue_FailsValidation(int isActive)
        {
            var cmd = ValidCommand();
            cmd.IsActive = isActive;
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.IsActive);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public async Task IsActive_ValidValues_PassesValidation(int isActive)
        {
            var cmd = ValidCommand();
            cmd.IsActive = isActive;
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldNotHaveValidationErrorFor(x => x.IsActive);
        }

        // ── IsDefault / DefaultAlreadyExists Rules ────────────────────────────

        [Fact]
        public async Task IsDefault_True_DefaultAlreadyExists_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.IsDefault = true;
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(new DispatchAddressMappingDto { Id = 1, PartyId = 1, UsageTypeId = 1 });
            _mockQueryRepo.Setup(r => r.DefaultAlreadyExistsAsync(1, 1, 1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveAnyValidationError();
        }
    }
}
