using PartyManagement.Domain.Common;
using PartyManagement.Domain.Entities;
using static PartyManagement.Domain.Common.BaseEntity;
using Xunit;

namespace PartyManagement.UnitTests.Domain
{
    public class BankMasterEntityTests
    {
        [Fact]
        public void BankMaster_DefaultIsActive_ShouldBeActive()
        {
            var entity = new BankMaster();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void BankMaster_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new BankMaster();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void BankMaster_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(BankMaster)).Should().BeTrue();
        }

        [Fact]
        public void BankMaster_Properties_ShouldBeAssignable()
        {
            var entity = new BankMaster
            {
                Id = 1,
                BankCode = "BNK001",
                BankName = "ICICI Bank"
            };

            entity.Id.Should().Be(1);
            entity.BankCode.Should().Be("BNK001");
            entity.BankName.Should().Be("ICICI Bank");
        }
    }
}
