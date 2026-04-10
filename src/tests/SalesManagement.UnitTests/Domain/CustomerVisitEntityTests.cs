using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain
{
    public class CustomerVisitEntityTests
    {
        [Fact]
        public void CustomerVisit_DefaultIsActive_ShouldBeActive()
        {
            var entity = new CustomerVisit();

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void CustomerVisit_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new CustomerVisit();

            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void CustomerVisit_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(CustomerVisit)).Should().BeTrue();
        }

        [Fact]
        public void CustomerVisit_Properties_ShouldBeAssignable()
        {
            var visitDateTime = DateTimeOffset.UtcNow;

            var entity = new CustomerVisit
            {
                Id = 1,
                CustomerId = 10,
                VisitTypeId = 20,
                VisitDateTime = visitDateTime,
                Latitude = 12.97m,
                Longitude = 77.59m,
                ImageName = "test-image.jpg",
                Remarks = "Test visit",
                MarketingOfficerId = 30
            };

            entity.Id.Should().Be(1);
            entity.CustomerId.Should().Be(10);
            entity.VisitTypeId.Should().Be(20);
            entity.VisitDateTime.Should().Be(visitDateTime);
            entity.Latitude.Should().Be(12.97m);
            entity.Longitude.Should().Be(77.59m);
            entity.ImageName.Should().Be("test-image.jpg");
            entity.Remarks.Should().Be("Test visit");
            entity.MarketingOfficerId.Should().Be(30);
        }

        [Fact]
        public void CustomerVisit_NullableProperties_ShouldAcceptNull()
        {
            var entity = new CustomerVisit
            {
                Latitude = null,
                Longitude = null,
                ImageName = null,
                Remarks = null
            };

            entity.Latitude.Should().BeNull();
            entity.Longitude.Should().BeNull();
            entity.ImageName.Should().BeNull();
            entity.Remarks.Should().BeNull();
        }

        [Fact]
        public void CustomerVisit_NullableProperties_ShouldAcceptValues()
        {
            var entity = new CustomerVisit
            {
                Latitude = -45.5m,
                Longitude = 120.3m,
                ImageName = "photo.png",
                Remarks = "Detailed remark"
            };

            entity.Latitude.Should().Be(-45.5m);
            entity.Longitude.Should().Be(120.3m);
            entity.ImageName.Should().Be("photo.png");
            entity.Remarks.Should().Be("Detailed remark");
        }

        [Fact]
        public void CustomerVisit_NavigationProperties_ShouldBeAssignable()
        {
            var visitType = new MiscMaster { Id = 1 };
            var officer = new MarketingOfficer { Id = 2 };

            var entity = new CustomerVisit
            {
                VisitType = visitType,
                MarketingOfficer = officer
            };

            entity.VisitType.Should().NotBeNull();
            entity.VisitType!.Id.Should().Be(1);
            entity.MarketingOfficer.Should().NotBeNull();
            entity.MarketingOfficer!.Id.Should().Be(2);
        }

        [Fact]
        public void CustomerVisit_ChildCollection_ShouldBeAssignable()
        {
            var products = new List<CustomerVisitProduct>
            {
                new() { Id = 1, ItemId = 100 },
                new() { Id = 2, ItemId = 200 }
            };

            var entity = new CustomerVisit
            {
                CustomerVisitProducts = products
            };

            entity.CustomerVisitProducts.Should().HaveCount(2);
        }

        [Fact]
        public void CustomerVisit_IsActive_CanBeSetToInactive()
        {
            var entity = new CustomerVisit
            {
                IsActive = Status.Inactive
            };

            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void CustomerVisit_IsDeleted_CanBeSetToDeleted()
        {
            var entity = new CustomerVisit
            {
                IsDeleted = IsDelete.Deleted
            };

            entity.IsDeleted.Should().Be(IsDelete.Deleted);
        }
    }
}
