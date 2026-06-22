using FinanceManagement.Domain.Common;
using FinanceManagement.Domain.Entities;

namespace FinanceManagement.UnitTests.Domain
{
    public class VoucherTypeMasterEntityTests
    {
        [Fact]
        public void VoucherTypeMaster_DefaultIsActive_ShouldBeActive()
        {
            new VoucherTypeMaster().IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void VoucherTypeMaster_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            new VoucherTypeMaster().IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void VoucherTypeMaster_DefaultIsSystem_ShouldBeFalse()
        {
            new VoucherTypeMaster().IsSystem.Should().BeFalse();
        }

        [Fact]
        public void VoucherTypeMaster_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(VoucherTypeMaster)).Should().BeTrue();
        }

        [Fact]
        public void VoucherTypeMaster_Properties_ShouldBeAssignable()
        {
            var entity = new VoucherTypeMaster
            {
                Id = 1,
                CompanyId = 7,
                VoucherTypeCode = "JV",
                VoucherTypeName = "Journal Voucher",
                NumberPadding = 4,
                IsSystem = true
            };

            entity.Id.Should().Be(1);
            entity.CompanyId.Should().Be(7);
            entity.VoucherTypeCode.Should().Be("JV");
            entity.VoucherTypeName.Should().Be("Journal Voucher");
            entity.NumberPadding.Should().Be(4);
            entity.IsSystem.Should().BeTrue();
        }

        [Fact]
        public void VoucherTypeMaster_NavigationCollections_ShouldBeAssignable()
        {
            var entity = new VoucherTypeMaster
            {
                AllowedAccountTypes = new List<VoucherTypeAccountType> { new() { AccountTypeId = 1 } },
                NumberSeries = new List<VoucherTypeNumberSeries> { new() { FinancialYearId = 3 } }
            };

            entity.AllowedAccountTypes.Should().HaveCount(1);
            entity.NumberSeries.Should().HaveCount(1);
        }
    }
}
