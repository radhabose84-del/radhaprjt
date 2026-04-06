using Contracts.Dtos.Lookups.Party;
using Contracts.Interfaces.Lookups.Party;
using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IDispatchAddressMapping;
using SalesManagement.Application.DispatchAddressMapping.Commands.CreateDispatchAddressMapping;
using SalesManagement.Presentation.Validation.DispatchAddressMapping;

namespace SalesManagement.UnitTests.Validators.DispatchAddressMapping
{
    public sealed class CreateDispatchAddressMappingCommandValidatorTests
    {
        private readonly Mock<IDispatchAddressMappingQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IPartyLookup> _mockPartyLookup = new(MockBehavior.Loose);

        private CreateDispatchAddressMappingCommandValidator CreateValidator()
            => new(_mockQueryRepo.Object, _mockPartyLookup.Object);

        private void SetupAllAsyncMocks(int partyId = 1, int dispatchAddressId = 1, int usageTypeId = 1)
        {
            _mockPartyLookup.Setup(p => p.GetByIdAsync(partyId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PartyLookupDto { Id = partyId, PartyName = "Test Party" });
            _mockQueryRepo.Setup(r => r.DispatchAddressExistsAsync(dispatchAddressId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(usageTypeId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.CompositeKeyExistsAsync(partyId, dispatchAddressId, usageTypeId, null)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.DefaultAlreadyExistsAsync(partyId, usageTypeId, null)).ReturnsAsync(false);
        }

        private static CreateDispatchAddressMappingCommand ValidCommand() => new()
        {
            PartyId = 1,
            DispatchAddressId = 1,
            UsageTypeId = 1,
            IsDefault = false
        };

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(ValidCommand());

            result.ShouldNotHaveAnyValidationErrors();
        }

        // ── PartyId Rules ─────────────────────────────────────────────────────

        [Fact]
        public async Task PartyId_Zero_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.PartyId = 0;
            _mockQueryRepo.Setup(r => r.DispatchAddressExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.PartyId);
        }

        [Fact]
        public async Task PartyId_NotFound_FailsValidation()
        {
            var cmd = ValidCommand();
            _mockPartyLookup.Setup(p => p.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PartyLookupDto?)null);
            _mockQueryRepo.Setup(r => r.DispatchAddressExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.CompositeKeyExistsAsync(1, 1, 1, null)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.DefaultAlreadyExistsAsync(1, 1, null)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.PartyId);
        }

        // ── DispatchAddressId Rules ───────────────────────────────────────────

        [Fact]
        public async Task DispatchAddressId_Zero_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.DispatchAddressId = 0;
            _mockPartyLookup.Setup(p => p.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PartyLookupDto { Id = 1 });
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.DispatchAddressId);
        }

        [Fact]
        public async Task DispatchAddressId_NotFound_FailsValidation()
        {
            var cmd = ValidCommand();
            _mockPartyLookup.Setup(p => p.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PartyLookupDto { Id = 1 });
            _mockQueryRepo.Setup(r => r.DispatchAddressExistsAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.CompositeKeyExistsAsync(1, 1, 1, null)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.DefaultAlreadyExistsAsync(1, 1, null)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.DispatchAddressId);
        }

        // ── UsageTypeId Rules ─────────────────────────────────────────────────

        [Fact]
        public async Task UsageTypeId_Zero_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.UsageTypeId = 0;
            _mockPartyLookup.Setup(p => p.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PartyLookupDto { Id = 1 });
            _mockQueryRepo.Setup(r => r.DispatchAddressExistsAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.UsageTypeId);
        }

        // ── CompositeKey / AlreadyExists Rules ────────────────────────────────

        [Fact]
        public async Task CompositeKey_AlreadyExists_FailsValidation()
        {
            SetupAllAsyncMocks();
            _mockQueryRepo.Setup(r => r.CompositeKeyExistsAsync(1, 1, 1, null)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(ValidCommand());

            result.ShouldHaveAnyValidationError();
        }

        // ── IsDefault / DefaultAlreadyExists Rules ────────────────────────────

        [Fact]
        public async Task IsDefault_True_DefaultAlreadyExists_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.IsDefault = true;
            SetupAllAsyncMocks();
            _mockQueryRepo.Setup(r => r.DefaultAlreadyExistsAsync(1, 1, null)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveAnyValidationError();
        }
    }
}
