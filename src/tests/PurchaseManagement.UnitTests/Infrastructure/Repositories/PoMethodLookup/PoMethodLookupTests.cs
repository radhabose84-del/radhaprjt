using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities;
using SUT = PurchaseManagement.Infrastructure.Repositories.PoMethodLookup.PoMethodLookup;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Infrastructure.Repositories.PoMethodLookup
{
    /// <summary>
    /// PoMethodLookup is a thin composition over IMiscMasterQueryRepository — it resolves
    /// the MiscMaster ids for the "POMethod" misc type's "Local" and "Import" entries,
    /// and exposes typed accessors. This test set verifies:
    ///   1. All public methods route through GetMiscMasterByName(POMethod, Local|Import).
    ///   2. Id classification (IsLocal/IsImport/IsValid) returns the correct boolean.
    ///   3. Misconfigured misc data raises InvalidOperationException with a clear message.
    /// </summary>
    public sealed class PoMethodLookupTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _miscRepo = new(MockBehavior.Strict);

        private SUT CreateSut() => new(_miscRepo.Object);

        private static MiscMaster Misc(int id, string code) =>
            new()
            {
                Id = id,
                Code = code,
                Description = code,
                MiscTypeId = 99,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private void SetupHappyPath(int localId = 11, int importId = 22, int contractId = 0)
        {
            _miscRepo
                .Setup(r => r.GetMiscMasterByName(MiscEnumEntity.POMethod, MiscEnumEntity.Local))
                .ReturnsAsync(Misc(localId, MiscEnumEntity.Local));

            _miscRepo
                .Setup(r => r.GetMiscMasterByName(MiscEnumEntity.POMethod, MiscEnumEntity.Import))
                .ReturnsAsync(Misc(importId, MiscEnumEntity.Import));

            _miscRepo
                .Setup(r => r.GetMiscMasterByName(MiscEnumEntity.POMethod, MiscEnumEntity.Contract))
                .ReturnsAsync(contractId > 0 ? Misc(contractId, MiscEnumEntity.Contract) : null!);
        }

        // ── GetLocalIdAsync / GetImportIdAsync ────────────────────────────────

        [Fact]
        public async Task GetLocalIdAsync_Should_Return_Local_Id_From_MiscMaster()
        {
            SetupHappyPath(localId: 100);

            var result = await CreateSut().GetLocalIdAsync(CancellationToken.None);

            result.Should().Be(100);
        }

        [Fact]
        public async Task GetImportIdAsync_Should_Return_Import_Id_From_MiscMaster()
        {
            SetupHappyPath(importId: 200);

            var result = await CreateSut().GetImportIdAsync(CancellationToken.None);

            result.Should().Be(200);
        }

        // ── IsLocalAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task IsLocalAsync_Should_Return_True_When_Id_Matches_Local()
        {
            SetupHappyPath(localId: 100, importId: 200);

            var result = await CreateSut().IsLocalAsync(100, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsLocalAsync_Should_Return_False_When_Id_Matches_Import()
        {
            SetupHappyPath(localId: 100, importId: 200);

            var result = await CreateSut().IsLocalAsync(200, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsLocalAsync_Should_Return_False_When_Id_Is_Unrelated()
        {
            SetupHappyPath(localId: 100, importId: 200);

            var result = await CreateSut().IsLocalAsync(999, CancellationToken.None);

            result.Should().BeFalse();
        }

        // ── IsImportAsync ─────────────────────────────────────────────────────

        [Fact]
        public async Task IsImportAsync_Should_Return_True_When_Id_Matches_Import()
        {
            SetupHappyPath(localId: 100, importId: 200);

            var result = await CreateSut().IsImportAsync(200, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsImportAsync_Should_Return_False_When_Id_Matches_Local()
        {
            SetupHappyPath(localId: 100, importId: 200);

            var result = await CreateSut().IsImportAsync(100, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsImportAsync_Should_Return_False_When_Id_Is_Unrelated()
        {
            SetupHappyPath(localId: 100, importId: 200);

            var result = await CreateSut().IsImportAsync(999, CancellationToken.None);

            result.Should().BeFalse();
        }

        // ── IsValidAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task IsValidAsync_Should_Return_True_For_Local_Id()
        {
            SetupHappyPath(localId: 100, importId: 200);

            var result = await CreateSut().IsValidAsync(100, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsValidAsync_Should_Return_True_For_Import_Id()
        {
            SetupHappyPath(localId: 100, importId: 200);

            var result = await CreateSut().IsValidAsync(200, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsValidAsync_Should_Return_False_For_Unrelated_Id()
        {
            SetupHappyPath(localId: 100, importId: 200);

            var result = await CreateSut().IsValidAsync(999, CancellationToken.None);

            result.Should().BeFalse();
        }

        // ── Misconfiguration error path ───────────────────────────────────────

        [Fact]
        public async Task GetLocalIdAsync_Should_Throw_When_Local_MiscMaster_Missing()
        {
            _miscRepo
                .Setup(r => r.GetMiscMasterByName(MiscEnumEntity.POMethod, MiscEnumEntity.Local))
                .ReturnsAsync((MiscMaster)null!);
            _miscRepo
                .Setup(r => r.GetMiscMasterByName(MiscEnumEntity.POMethod, MiscEnumEntity.Import))
                .ReturnsAsync(Misc(22, MiscEnumEntity.Import));
            _miscRepo
                .Setup(r => r.GetMiscMasterByName(MiscEnumEntity.POMethod, MiscEnumEntity.Contract))
                .ReturnsAsync((MiscMaster)null!);

            Func<Task> act = async () => await CreateSut().GetLocalIdAsync(CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*POMethod*not configured*");
        }

        [Fact]
        public async Task GetImportIdAsync_Should_Throw_When_Import_MiscMaster_Has_Invalid_Id()
        {
            _miscRepo
                .Setup(r => r.GetMiscMasterByName(MiscEnumEntity.POMethod, MiscEnumEntity.Local))
                .ReturnsAsync(Misc(11, MiscEnumEntity.Local));
            _miscRepo
                .Setup(r => r.GetMiscMasterByName(MiscEnumEntity.POMethod, MiscEnumEntity.Import))
                .ReturnsAsync(Misc(0, MiscEnumEntity.Import));   // Id = 0 is treated as "not configured"
            _miscRepo
                .Setup(r => r.GetMiscMasterByName(MiscEnumEntity.POMethod, MiscEnumEntity.Contract))
                .ReturnsAsync((MiscMaster)null!);

            Func<Task> act = async () => await CreateSut().GetImportIdAsync(CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*POMethod*not configured*");
        }

        [Fact]
        public async Task IsValidAsync_Should_Throw_When_MiscMaster_Misconfigured()
        {
            _miscRepo
                .Setup(r => r.GetMiscMasterByName(MiscEnumEntity.POMethod, MiscEnumEntity.Local))
                .ReturnsAsync((MiscMaster)null!);
            _miscRepo
                .Setup(r => r.GetMiscMasterByName(MiscEnumEntity.POMethod, MiscEnumEntity.Import))
                .ReturnsAsync((MiscMaster)null!);
            _miscRepo
                .Setup(r => r.GetMiscMasterByName(MiscEnumEntity.POMethod, MiscEnumEntity.Contract))
                .ReturnsAsync((MiscMaster)null!);

            Func<Task> act = async () => await CreateSut().IsValidAsync(1, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        // ── No-caching contract ───────────────────────────────────────────────

        [Fact]
        public async Task Each_Call_Should_Reload_From_Repository_No_Caching()
        {
            SetupHappyPath();

            var sut = CreateSut();
            await sut.GetLocalIdAsync(CancellationToken.None);
            await sut.GetImportIdAsync(CancellationToken.None);
            await sut.IsLocalAsync(11, CancellationToken.None);

            // 3 calls × 3 misc lookups each = 9 total
            _miscRepo.Verify(
                r => r.GetMiscMasterByName(MiscEnumEntity.POMethod, MiscEnumEntity.Local),
                Times.Exactly(3));
            _miscRepo.Verify(
                r => r.GetMiscMasterByName(MiscEnumEntity.POMethod, MiscEnumEntity.Import),
                Times.Exactly(3));
            _miscRepo.Verify(
                r => r.GetMiscMasterByName(MiscEnumEntity.POMethod, MiscEnumEntity.Contract),
                Times.Exactly(3));
        }
    }
}
