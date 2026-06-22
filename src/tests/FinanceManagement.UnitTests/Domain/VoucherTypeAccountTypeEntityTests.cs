using FinanceManagement.Domain.Common;
using FinanceManagement.Domain.Entities;

namespace FinanceManagement.UnitTests.Domain
{
    public class VoucherTypeAccountTypeEntityTests
    {
        [Fact]
        public void VoucherTypeAccountType_DefaultIsActive_ShouldBeActive()
        {
            new VoucherTypeAccountType().IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void VoucherTypeAccountType_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            new VoucherTypeAccountType().IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void VoucherTypeAccountType_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(VoucherTypeAccountType)).Should().BeTrue();
        }

        [Fact]
        public void VoucherTypeAccountType_Properties_ShouldBeAssignable()
        {
            var entity = new VoucherTypeAccountType { Id = 1, VoucherTypeId = 2, AccountTypeId = 3 };

            entity.Id.Should().Be(1);
            entity.VoucherTypeId.Should().Be(2);
            entity.AccountTypeId.Should().Be(3);
        }
    }
}
