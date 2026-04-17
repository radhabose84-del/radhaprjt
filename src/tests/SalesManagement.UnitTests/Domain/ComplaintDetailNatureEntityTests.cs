using SalesManagement.Domain.Entities;

namespace SalesManagement.UnitTests.Domain;

public class ComplaintDetailNatureEntityTests
{
    [Fact]
    public void Properties_ShouldBeAssignable()
    {
        var entity = new ComplaintDetailNature
        {
            Id = 1,
            ComplaintDetailId = 10,
            NatureOfComplaintId = 5
        };

        entity.Id.Should().Be(1);
        entity.ComplaintDetailId.Should().Be(10);
        entity.NatureOfComplaintId.Should().Be(5);
    }

    [Fact]
    public void NullableNavigationProperties_ShouldAcceptNull()
    {
        var entity = new ComplaintDetailNature
        {
            NatureOfComplaintMisc = null,
            ComplaintDetail = null
        };

        entity.NatureOfComplaintMisc.Should().BeNull();
        entity.ComplaintDetail.Should().BeNull();
    }
}
