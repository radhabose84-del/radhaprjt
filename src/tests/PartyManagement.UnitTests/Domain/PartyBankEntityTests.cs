using PartyManagement.Domain.Entities;

namespace PartyManagement.UnitTests.Domain
{
    public class PartyBankEntityTests
    {
        [Fact]
        public void PartyBank_ShouldNotInheritFromBaseEntity()
        {
            typeof(PartyManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(PartyBank)).Should().BeFalse();
        }

        [Fact]
        public void PartyBank_Properties_ShouldBeAssignable()
        {
            var entity = new PartyBank
            {
                Id = 1,
                PartyId = 10,
                BankName = "State Bank",
                BankAccountNumber = "1234567890",
                BankBranch = "Main Branch",
                IFSCCode = "SBIN0001234",
                SWIFTCode = "SBININBB",
                AccountTypeId = 2,
                IsDefaultAccount = true,
                IsPrimaryAccount = false
            };

            entity.Id.Should().Be(1);
            entity.PartyId.Should().Be(10);
            entity.BankName.Should().Be("State Bank");
            entity.BankAccountNumber.Should().Be("1234567890");
            entity.BankBranch.Should().Be("Main Branch");
            entity.IFSCCode.Should().Be("SBIN0001234");
            entity.SWIFTCode.Should().Be("SBININBB");
            entity.AccountTypeId.Should().Be(2);
            entity.IsDefaultAccount.Should().BeTrue();
            entity.IsPrimaryAccount.Should().BeFalse();
        }

        [Fact]
        public void PartyBank_NullableProperties_ShouldAcceptNull()
        {
            var entity = new PartyBank
            {
                BankName = null,
                BankAccountNumber = null,
                BankBranch = null,
                IFSCCode = null,
                SWIFTCode = null,
                AccountTypeId = null
            };

            entity.BankName.Should().BeNull();
            entity.BankAccountNumber.Should().BeNull();
            entity.BankBranch.Should().BeNull();
            entity.IFSCCode.Should().BeNull();
            entity.SWIFTCode.Should().BeNull();
            entity.AccountTypeId.Should().BeNull();
        }

        [Fact]
        public void PartyBank_BoolProperties_ShouldDefaultToFalse()
        {
            var entity = new PartyBank();

            entity.IsDefaultAccount.Should().BeFalse();
            entity.IsPrimaryAccount.Should().BeFalse();
        }

        [Fact]
        public void PartyBank_NavigationProperties_ShouldBeAssignable()
        {
            var party = new PartyMaster { Id = 10 };
            var miscMaster = new MiscMaster { Id = 2 };
            var entity = new PartyBank
            {
                PartyBankId = party,
                BankAccountType = miscMaster
            };

            entity.PartyBankId.Should().NotBeNull();
            entity.BankAccountType.Should().NotBeNull();
        }
    }
}
