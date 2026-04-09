using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain;

public class StoTypeMasterEntityTests
{
    [Fact]
    public void StoTypeMaster_DefaultIsActive_ShouldBeActive()
    {
        var entity = new StoTypeMaster();
        entity.IsActive.Should().Be(Status.Active);
    }

    [Fact]
    public void StoTypeMaster_DefaultIsDeleted_ShouldBeNotDeleted()
    {
        var entity = new StoTypeMaster();
        entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
    }

    [Fact]
    public void StoTypeMaster_ShouldInheritFromBaseEntity()
    {
        typeof(BaseEntity).IsAssignableFrom(typeof(StoTypeMaster)).Should().BeTrue();
    }

    [Fact]
    public void StoTypeMaster_Properties_ShouldBeAssignable()
    {
        var entity = new StoTypeMaster
        {
            Id = 1,
            StoTypeCode = "STO001",
            StoTypeName = "Standard STO",
            Description = "Standard Stock Transfer Order",
            PgiMovementTypeId = 10,
            GrMovementTypeId = 20
        };

        entity.Id.Should().Be(1);
        entity.StoTypeCode.Should().Be("STO001");
        entity.StoTypeName.Should().Be("Standard STO");
        entity.Description.Should().Be("Standard Stock Transfer Order");
        entity.PgiMovementTypeId.Should().Be(10);
        entity.GrMovementTypeId.Should().Be(20);
    }

    [Fact]
    public void StoTypeMaster_NullableProperties_ShouldAcceptNull()
    {
        var entity = new StoTypeMaster
        {
            StoTypeCode = null,
            StoTypeName = null,
            Description = null
        };

        entity.StoTypeCode.Should().BeNull();
        entity.StoTypeName.Should().BeNull();
        entity.Description.Should().BeNull();
    }

    [Fact]
    public void StoTypeMaster_NavigationProperties_ShouldBeAssignable()
    {
        var entity = new StoTypeMaster
        {
            PgiMovementType = new MovementTypeConfig(),
            GrMovementType = new MovementTypeConfig(),
            StoHeaders = new List<StoHeader>()
        };

        entity.PgiMovementType.Should().NotBeNull();
        entity.GrMovementType.Should().NotBeNull();
        entity.StoHeaders.Should().NotBeNull();
    }
}
