using PartyManagement.Domain.Entities;

namespace PartyManagement.UnitTests.Domain
{
    public class PartyDocumentEntityTests
    {
        [Fact]
        public void PartyDocument_ShouldNotInheritFromBaseEntity()
        {
            typeof(PartyManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(PartyDocument)).Should().BeFalse();
        }

        [Fact]
        public void PartyDocument_Properties_ShouldBeAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var entity = new PartyDocument
            {
                Id = 1,
                PartyId = 10,
                DocumentId = 5,
                FileName = "invoice.pdf",
                UploadedDate = now
            };

            entity.Id.Should().Be(1);
            entity.PartyId.Should().Be(10);
            entity.DocumentId.Should().Be(5);
            entity.FileName.Should().Be("invoice.pdf");
            entity.UploadedDate.Should().Be(now);
        }

        [Fact]
        public void PartyDocument_NullableProperties_ShouldAcceptNull()
        {
            var entity = new PartyDocument
            {
                FileName = null
            };

            entity.FileName.Should().BeNull();
        }

        [Fact]
        public void PartyDocument_NavigationProperties_ShouldBeAssignable()
        {
            var party = new PartyMaster { Id = 10 };
            var docType = new MiscMaster { Id = 5 };

            var entity = new PartyDocument
            {
                PartyDocumentId = party,
                DocumentTypeMisc = docType
            };

            entity.PartyDocumentId.Should().NotBeNull();
            entity.DocumentTypeMisc.Should().NotBeNull();
        }
    }
}
