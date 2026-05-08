using SalesManagement.Domain.Common;

namespace SalesManagement.UnitTests.Domain;

public class MiscEnumEntityTests
{
    // --- StockStatus group (consolidated from BagStatus 2026-05-07) ---

    [Fact]
    public void StockStatus_ShouldBe_StockStatus()
        => MiscEnumEntity.StockStatus.Should().Be("StockStatus");

    [Fact]
    public void StockStatusDefect_ShouldBe_DEFECT()
        => MiscEnumEntity.StockStatusDefect.Should().Be("DEFECT");

    [Fact]
    public void StockStatusDamaged_ShouldBe_DAMAGED()
        => MiscEnumEntity.StockStatusDamaged.Should().Be("DAMAGED");

    [Fact]
    public void StockStatusYarnMismatch_ShouldBe_YARN_MISMATCH()
        => MiscEnumEntity.StockStatusYarnMismatch.Should().Be("YARN MISMATCH");

    // --- ReturnStatus constants (used alongside StockStatus in Sales Return) ---

    [Fact]
    public void ReturnStatus_ShouldBe_ReturnStatus()
        => MiscEnumEntity.ReturnStatus.Should().Be("ReturnStatus");

    [Fact]
    public void ReturnStatusPending_ShouldBe_Pending()
        => MiscEnumEntity.ReturnStatusPending.Should().Be("Pending");

    [Fact]
    public void ReturnStatusReceived_ShouldBe_Received()
        => MiscEnumEntity.ReturnStatusReceived.Should().Be("Received");

    [Fact]
    public void ReturnStatusFullyReturned_ShouldBe_FullyReturned()
        => MiscEnumEntity.ReturnStatusFullyReturned.Should().Be("FullyReturned");
}
