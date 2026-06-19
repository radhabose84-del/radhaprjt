using FinanceManagement.Domain.Common;
using FinanceManagement.Domain.Entities;

namespace FinanceManagement.UnitTests.Domain
{
    public class VoucherTypeNumberSeriesEntityTests
    {
        [Fact]
        public void VoucherTypeNumberSeries_DefaultIsActive_ShouldBeActive()
        {
            new VoucherTypeNumberSeries().IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void VoucherTypeNumberSeries_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            new VoucherTypeNumberSeries().IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void VoucherTypeNumberSeries_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(VoucherTypeNumberSeries)).Should().BeTrue();
        }

        [Fact]
        public void VoucherTypeNumberSeries_Properties_ShouldBeAssignable()
        {
            var entity = new VoucherTypeNumberSeries { Id = 1, VoucherTypeId = 2, FinancialYearId = 3, LastUsedNumber = 427 };

            entity.Id.Should().Be(1);
            entity.VoucherTypeId.Should().Be(2);
            entity.FinancialYearId.Should().Be(3);
            entity.LastUsedNumber.Should().Be(427);
        }
    }
}
