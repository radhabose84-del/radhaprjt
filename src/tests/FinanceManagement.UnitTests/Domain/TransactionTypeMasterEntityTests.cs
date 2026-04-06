using FinanceManagement.Domain.Common;
using FinanceManagement.Domain.Entities;

namespace FinanceManagement.UnitTests.Domain
{
    public class TransactionTypeMasterEntityTests
    {
        [Fact]
        public void TransactionTypeMaster_DefaultIsActive_ShouldBeActive()
        {
            var entity = new TransactionTypeMaster();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void TransactionTypeMaster_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new TransactionTypeMaster();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void TransactionTypeMaster_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(TransactionTypeMaster)).Should().BeTrue();
        }

        [Fact]
        public void TransactionTypeMaster_Properties_ShouldBeAssignable()
        {
            var entity = new TransactionTypeMaster
            {
                Id = 1,
                UnitId = 10,
                ModuleId = 5,
                MenuId = 3,
                TypeName = "Invoice",
                ShortName = "INV",
                Description = "Invoice Type"
            };

            entity.Id.Should().Be(1);
            entity.UnitId.Should().Be(10);
            entity.ModuleId.Should().Be(5);
            entity.MenuId.Should().Be(3);
            entity.TypeName.Should().Be("Invoice");
            entity.ShortName.Should().Be("INV");
            entity.Description.Should().Be("Invoice Type");
        }

        [Fact]
        public void TransactionTypeMaster_NullableProperties_ShouldAcceptNull()
        {
            var entity = new TransactionTypeMaster
            {
                TypeName = null,
                ShortName = null,
                Description = null
            };

            entity.TypeName.Should().BeNull();
            entity.ShortName.Should().BeNull();
            entity.Description.Should().BeNull();
        }

        [Fact]
        public void TransactionTypeMaster_NavigationProperty_ShouldBeAssignable()
        {
            var entity = new TransactionTypeMaster
            {
                DocumentSequences = new List<DocumentSequence>
                {
                    new DocumentSequence { Id = 1, TransactionTypeId = 1 }
                }
            };

            entity.DocumentSequences.Should().HaveCount(1);
        }

        [Fact]
        public void TransactionTypeMaster_NavigationProperty_ShouldAcceptNull()
        {
            var entity = new TransactionTypeMaster { DocumentSequences = null };
            entity.DocumentSequences.Should().BeNull();
        }
    }
}
