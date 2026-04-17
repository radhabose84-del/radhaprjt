using UserManagement.Application.MiscMaster.Queries.GetMiscMasterToDownload;

namespace UserManagement.UnitTests.Application.MiscMaster.Queries;

public sealed class GetMiscasterToExportQueryHandlerTests
{
    [Fact]
    public void Handler_ShouldBeInstantiable()
    {
        // GetMiscasterToExportQueryHandler is an empty stub class with no Handle method.
        // This test verifies it can be instantiated.
        var handler = new GetMiscasterToExportQueryHandler();
        handler.Should().NotBeNull();
    }
}
