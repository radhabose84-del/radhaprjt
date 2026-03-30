using PartyManagement.Domain.Common;
using PartyManagement.Domain.Entities;
using static PartyManagement.Domain.Common.BaseEntity;
using Xunit;

namespace PartyManagement.UnitTests.Domain
{
    public class PartyMasterEntityTests
    {
        [Fact]
        public void PartyMaster_DefaultIsActive_ShouldBeActive()
        {
            var entity = new PartyMaster();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void PartyMaster_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new PartyMaster();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void PartyMaster_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(PartyMaster)).Should().BeTrue();
        }

        [Fact]
        public void PartyMaster_Properties_ShouldBeAssignable()
        {
            var entity = new PartyMaster
            {
                Id = 1,
                PartyCode = "PAR001",
                PartyName = "Test Party",
                GSTNumber = "22AAAAA1234A1Z5",
                PAN = "ABCDE1234F"
            };

            entity.Id.Should().Be(1);
            entity.PartyCode.Should().Be("PAR001");
            entity.PartyName.Should().Be("Test Party");
            entity.GSTNumber.Should().Be("22AAAAA1234A1Z5");
        }

        [Fact]
        public void PartyMaster_NullableProperties_ShouldAcceptNull()
        {
            var entity = new PartyMaster
            {
                PartyZoneId = null,
                GSTNumber = null,
                PAN = null,
                CreditLimit = null
            };

            entity.PartyZoneId.Should().BeNull();
            entity.GSTNumber.Should().BeNull();
            entity.PAN.Should().BeNull();
        }

        [Fact]
        public void PartyMaster_BoolProperties_ShouldBeAssignable()
        {
            var entity = new PartyMaster
            {
                IsMsmeCompliant = true,
                IsTDSApplicable = true,
                IsGroup = false,
                IsSubsidiary = false
            };

            entity.IsMsmeCompliant.Should().BeTrue();
            entity.IsTDSApplicable.Should().BeTrue();
            entity.IsGroup.Should().BeFalse();
        }
    }
}
