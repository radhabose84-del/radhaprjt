using AutoMapper;
using SalesManagement.Application.Common.Mappings;
using SalesManagement.Application.SalesOrganisation.Commands.CreateSalesOrganisation;
using SalesManagement.Application.SalesOrganisation.Commands.UpdateSalesOrganisation;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Mappings
{
    public sealed class SalesOrganisationProfileTests
    {
        private readonly IMapper _mapper;

        public SalesOrganisationProfileTests()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<SalesOrganisationProfile>());
            _mapper = config.CreateMapper();
        }


        [Fact]
        public void CreateCommand_To_Entity_MapsFields()
        {
            var command = new CreateSalesOrganisationCommand
            {
                SalesOrganisationCode = "SO001",
                SalesOrganisationName = "Test Org",
                CompanyId = 1,
                Description = "Test Description"
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.SalesOrganisation>(command);

            entity.SalesOrganisationCode.Should().Be("SO001");
            entity.SalesOrganisationName.Should().Be("Test Org");
            entity.CompanyId.Should().Be(1);
            entity.Description.Should().Be("Test Description");
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsActive_Active()
        {
            var command = new CreateSalesOrganisationCommand
            {
                SalesOrganisationCode = "SO001",
                SalesOrganisationName = "Test Org",
                CompanyId = 1
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.SalesOrganisation>(command);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsDeleted_NotDeleted()
        {
            var command = new CreateSalesOrganisationCommand
            {
                SalesOrganisationCode = "SO001",
                SalesOrganisationName = "Test Org",
                CompanyId = 1
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.SalesOrganisation>(command);

            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void UpdateCommand_IsActive1_MapsTo_StatusActive()
        {
            var command = new UpdateSalesOrganisationCommand
            {
                Id = 1,
                SalesOrganisationName = "Updated Org",
                CompanyId = 1,
                IsActive = 1
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.SalesOrganisation>(command);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void UpdateCommand_IsActive0_MapsTo_StatusInactive()
        {
            var command = new UpdateSalesOrganisationCommand
            {
                Id = 1,
                SalesOrganisationName = "Updated Org",
                CompanyId = 1,
                IsActive = 0
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.SalesOrganisation>(command);

            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void UpdateCommand_To_Entity_MapsFields()
        {
            var command = new UpdateSalesOrganisationCommand
            {
                Id = 5,
                SalesOrganisationName = "Updated Name",
                CompanyId = 2,
                Description = "Updated Desc",
                IsActive = 1
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.SalesOrganisation>(command);

            entity.Id.Should().Be(5);
            entity.SalesOrganisationName.Should().Be("Updated Name");
            entity.CompanyId.Should().Be(2);
            entity.Description.Should().Be("Updated Desc");
        }
    }
}
