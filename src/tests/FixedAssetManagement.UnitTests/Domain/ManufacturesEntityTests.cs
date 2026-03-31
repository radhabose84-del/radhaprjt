using FAM.Domain.Common;
using FAM.Domain.Entities;
using static FAM.Domain.Common.BaseEntity;

namespace FixedAssetManagement.UnitTests.Domain
{
    public sealed class ManufacturesEntityTests
    {
        [Fact]
        public void Manufactures_DefaultIsActive_ShouldBeActive()
        {
            var entity = new Manufactures();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void Manufactures_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new Manufactures();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void Manufactures_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(Manufactures)).Should().BeTrue();
        }

        [Fact]
        public void Manufactures_Properties_ShouldBeAssignable()
        {
            var entity = new Manufactures
            {
                Id = 1,
                Code = "MFG001",
                ManufactureName = "TestManufacture",
                CountryId = 1,
                StateId = 1,
                CityId = 1
            };

            entity.Id.Should().Be(1);
            entity.Code.Should().Be("MFG001");
            entity.ManufactureName.Should().Be("TestManufacture");
            entity.CountryId.Should().Be(1);
        }

        [Fact]
        public void Manufactures_NullableProperties_ShouldAcceptNull()
        {
            var entity = new Manufactures
            {
                Code = null,
                ManufactureName = null,
                ManufactureType = null,
                AddressLine1 = null,
                AddressLine2 = null,
                PinCode = null,
                PersonName = null,
                PhoneNumber = null,
                Email = null
            };

            entity.Code.Should().BeNull();
            entity.ManufactureName.Should().BeNull();
        }
    }
}
