using FinanceManagement.Domain.Common;
using FinanceManagement.Domain.Entities;

namespace FinanceManagement.UnitTests.Domain
{
    public class GlAccountMasterEntityTests
    {
        [Fact]
        public void GlAccountMaster_DefaultIsActive_ShouldBeActive()
        {
            var entity = new GlAccountMaster();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void GlAccountMaster_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new GlAccountMaster();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void GlAccountMaster_DefaultIsCostCentreMandatory_ShouldBeFalse()
        {
            var entity = new GlAccountMaster();
            entity.IsCostCentreMandatory.Should().BeFalse();
        }

        [Fact]
        public void GlAccountMaster_DefaultIsProfitCentreMandatory_ShouldBeFalse()
        {
            var entity = new GlAccountMaster();
            entity.IsProfitCentreMandatory.Should().BeFalse();
        }

        [Fact]
        public void GlAccountMaster_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(GlAccountMaster)).Should().BeTrue();
        }

        [Fact]
        public void GlAccountMaster_Properties_ShouldBeAssignable()
        {
            var entity = new GlAccountMaster
            {
                Id = 1,
                CompanyId = 7,
                AccountTypeId = 2,
                AccountGroupId = 3,
                AccountCode = "61001010",
                AccountName = "Yarn Sales - Domestic",
                NormalBalanceId = 4,
                CurrencyTypeId = 5,
                SubLedgerTypeId = 6,
                IsCostCentreMandatory = false,
                IsProfitCentreMandatory = true,
                IsTaxRelevant = false,
                IsInterCompany = false,
                IsReconciliationRequired = false
            };

            entity.Id.Should().Be(1);
            entity.CompanyId.Should().Be(7);
            entity.AccountCode.Should().Be("61001010");
            entity.AccountName.Should().Be("Yarn Sales - Domestic");
            entity.IsCostCentreMandatory.Should().BeFalse();
            entity.IsProfitCentreMandatory.Should().BeTrue();
        }

        [Fact]
        public void GlAccountMaster_NullableProperties_ShouldAcceptNull()
        {
            var entity = new GlAccountMaster
            {
                Description = null,
                ImportLogId = null,
                LastPostFreezeChangeOn = null
            };

            entity.Description.Should().BeNull();
            entity.ImportLogId.Should().BeNull();
            entity.LastPostFreezeChangeOn.Should().BeNull();
        }

        // ── US-GL02-10 Multi-Company COA ───────────────────────────────────────
        [Fact]
        public void GlAccountMaster_DefaultMultiCompanyFlags_ShouldBeFalse()
        {
            var entity = new GlAccountMaster();
            entity.IsGlobal.Should().BeFalse();
            entity.IsCompanyRestricted.Should().BeFalse();
            entity.IsLocalOverride.Should().BeFalse();
        }

        [Fact]
        public void GlAccountMaster_GlobalAccountId_ShouldDefaultNull_AndAcceptValue()
        {
            var entity = new GlAccountMaster();
            entity.GlobalAccountId.Should().BeNull();

            entity.GlobalAccountId = 99;
            entity.GlobalAccountId.Should().Be(99);
        }

        [Fact]
        public void GlAccountMaster_MultiCompanyFlags_ShouldBeAssignable()
        {
            var entity = new GlAccountMaster
            {
                IsGlobal = true,
                IsCompanyRestricted = true,
                IsLocalOverride = true
            };

            entity.IsGlobal.Should().BeTrue();
            entity.IsCompanyRestricted.Should().BeTrue();
            entity.IsLocalOverride.Should().BeTrue();
        }
    }
}
