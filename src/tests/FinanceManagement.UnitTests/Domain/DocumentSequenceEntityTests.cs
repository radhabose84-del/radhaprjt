using FinanceManagement.Domain.Common;
using FinanceManagement.Domain.Entities;

namespace FinanceManagement.UnitTests.Domain
{
    public class DocumentSequenceEntityTests
    {
        [Fact]
        public void DocumentSequence_DefaultIsActive_ShouldBeActive()
        {
            var entity = new DocumentSequence();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void DocumentSequence_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new DocumentSequence();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void DocumentSequence_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(DocumentSequence)).Should().BeTrue();
        }

        [Fact]
        public void DocumentSequence_Properties_ShouldBeAssignable()
        {
            var entity = new DocumentSequence
            {
                Id = 1,
                TransactionTypeId = 10,
                FinancialYearId = 2025,
                DocNo = 100
            };

            entity.Id.Should().Be(1);
            entity.TransactionTypeId.Should().Be(10);
            entity.FinancialYearId.Should().Be(2025);
            entity.DocNo.Should().Be(100);
        }

        [Fact]
        public void DocumentSequence_NavigationProperty_ShouldBeAssignable()
        {
            var master = new TransactionTypeMaster { Id = 5, TypeName = "Sales" };
            var entity = new DocumentSequence
            {
                TransactionTypeMaster = master
            };

            entity.TransactionTypeMaster.Should().NotBeNull();
            entity.TransactionTypeMaster!.Id.Should().Be(5);
        }

        [Fact]
        public void DocumentSequence_NavigationProperty_ShouldAcceptNull()
        {
            var entity = new DocumentSequence { TransactionTypeMaster = null };
            entity.TransactionTypeMaster.Should().BeNull();
        }
    }
}
