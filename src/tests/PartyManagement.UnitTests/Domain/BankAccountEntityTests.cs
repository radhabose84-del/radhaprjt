using PartyManagement.Domain.Common;
using PartyManagement.Domain.Entities;
using static PartyManagement.Domain.Common.BaseEntity;
using Xunit;

namespace PartyManagement.UnitTests.Domain
{
    public class BankAccountEntityTests
    {
        [Fact]
        public void BankAccount_DefaultIsActive_ShouldBeActive()
        {
            var entity = new BankAccount();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void BankAccount_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new BankAccount();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void BankAccount_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(BankAccount)).Should().BeTrue();
        }

        [Fact]
        public void BankAccount_Properties_ShouldBeAssignable()
        {
            var entity = new BankAccount
            {
                Id = 1,
                BankId = 2,
                AccountNumber = "1234567890",
                AccountHolderName = "Test Holder",
                BranchId = 3,
                AccountTypeId = 1,
                IsDefaultAccount = true,
                IsPrimaryAccount = false
            };

            entity.Id.Should().Be(1);
            entity.BankId.Should().Be(2);
            entity.AccountNumber.Should().Be("1234567890");
            entity.AccountHolderName.Should().Be("Test Holder");
            entity.IsDefaultAccount.Should().BeTrue();
        }

        [Fact]
        public void BankAccount_NullableProperties_ShouldAcceptNull()
        {
            var entity = new BankAccount
            {
                IFSCCode = null,
                SWIFTCode = null,
                IBan = null
            };

            entity.IFSCCode.Should().BeNull();
            entity.SWIFTCode.Should().BeNull();
            entity.IBan.Should().BeNull();
        }
    }
}
