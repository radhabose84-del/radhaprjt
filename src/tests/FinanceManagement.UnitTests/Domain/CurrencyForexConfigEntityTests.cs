using FinanceManagement.Domain.Common;
using FinanceManagement.Domain.Entities;

namespace FinanceManagement.UnitTests.Domain
{
    public class CurrencyForexConfigEntityTests
    {
        [Fact]
        public void CurrencyForexConfig_DefaultIsActive_ShouldBeActive()
        {
            var entity = new CurrencyForexConfig();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void CurrencyForexConfig_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new CurrencyForexConfig();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void CurrencyForexConfig_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(CurrencyForexConfig)).Should().BeTrue();
        }

        [Fact]
        public void CurrencyForexConfig_Properties_ShouldBeAssignable()
        {
            var entity = new CurrencyForexConfig
            {
                Id = 1,
                CompanyId = 7,
                CurrencyTypeCode = "FOREX",
                CurrencyTypeName = "Forex"
            };

            entity.Id.Should().Be(1);
            entity.CompanyId.Should().Be(7);
            entity.CurrencyTypeCode.Should().Be("FOREX");
            entity.CurrencyTypeName.Should().Be("Forex");
        }
    }
}
