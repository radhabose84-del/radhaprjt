using FinanceManagement.Domain.Common;
using FinanceManagement.Domain.Entities;

namespace FinanceManagement.UnitTests.Domain
{
    public class AccountGroupEntityTests
    {
        [Fact]
        public void AccountGroup_DefaultIsActive_ShouldBeActive()
        {
            var entity = new AccountGroup();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void AccountGroup_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new AccountGroup();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void AccountGroup_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(AccountGroup)).Should().BeTrue();
        }

        [Fact]
        public void AccountGroup_Properties_ShouldBeAssignable()
        {
            var entity = new AccountGroup
            {
                Id = 1,
                CompanyId = 7,
                GroupCode = "A-CA-INV-FF",
                GroupName = "Finished Goods — Fabric",
                ParentAccountGroupId = 5,
                Level = 4,
                IsLeaf = true,
                SortOrder = 3
            };

            entity.Id.Should().Be(1);
            entity.CompanyId.Should().Be(7);
            entity.GroupCode.Should().Be("A-CA-INV-FF");
            entity.GroupName.Should().Be("Finished Goods — Fabric");
            entity.ParentAccountGroupId.Should().Be(5);
            entity.Level.Should().Be(4);
            entity.IsLeaf.Should().BeTrue();
            entity.SortOrder.Should().Be(3);
        }

        [Fact]
        public void AccountGroup_SelfNavigation_ShouldBeAssignable()
        {
            var parent = new AccountGroup { Id = 5, GroupName = "Inventories" };
            var entity = new AccountGroup
            {
                ParentAccountGroup = parent,
                Children = new List<AccountGroup> { new() { Id = 9 } }
            };

            entity.ParentAccountGroup.Should().NotBeNull();
            entity.ParentAccountGroup!.Id.Should().Be(5);
            entity.Children.Should().ContainSingle();
        }

        [Fact]
        public void AccountGroup_ParentAccountGroupId_ShouldAcceptNullForRoot()
        {
            var entity = new AccountGroup { ParentAccountGroupId = null };
            entity.ParentAccountGroupId.Should().BeNull();
        }

        [Fact]
        public void AccountGroup_DefaultMaxDepth_ShouldBeFour()
        {
            AccountGroup.DefaultMaxDepth.Should().Be(4);
        }

        [Fact]
        public void AccountGroup_Level1GroupNames_ShouldContainStatutoryHeads()
        {
            AccountGroup.Level1GroupNames.Should()
                .BeEquivalentTo(new[] { "Assets", "Liabilities", "Equity", "Revenue", "Expenses" });
        }
    }
}
