using AutoMapper;
using SalesManagement.Application.Common.Mappings;
using SalesManagement.Application.MarketingOfficer.Commands.CreateMarketingOfficer;
using SalesManagement.Application.MarketingOfficer.Commands.UpdateMarketingOfficer;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Mappings
{
    public sealed class MarketingOfficerProfileTests
    {
        private readonly IMapper _mapper;

        public MarketingOfficerProfileTests()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MarketingOfficerProfile>());
            _mapper = config.CreateMapper();
        }


        [Fact]
        public void CreateCommand_To_Entity_MapsFields()
        {
            var command = new CreateMarketingOfficerCommand
            {
                EmployeeNo = "EMP001",
                EmployeeName = "John Doe",
                MobileNo = "9876543210",
                Email = "john@test.com",
                Unit = "Unit A",
                Department = "Sales",
                Designation = "Officer",
                SalesOfficeId = 1
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.MarketingOfficer>(command);

            entity.EmployeeNo.Should().Be("EMP001");
            entity.EmployeeName.Should().Be("John Doe");
            entity.MobileNo.Should().Be("9876543210");
            entity.Email.Should().Be("john@test.com");
            entity.Unit.Should().Be("Unit A");
            entity.Department.Should().Be("Sales");
            entity.Designation.Should().Be("Officer");
            entity.SalesOfficeId.Should().Be(1);
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsActive_Active()
        {
            var command = new CreateMarketingOfficerCommand
            {
                EmployeeNo = "EMP001",
                EmployeeName = "John Doe",
                SalesOfficeId = 1
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.MarketingOfficer>(command);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsDeleted_NotDeleted()
        {
            var command = new CreateMarketingOfficerCommand
            {
                EmployeeNo = "EMP001",
                EmployeeName = "John Doe",
                SalesOfficeId = 1
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.MarketingOfficer>(command);

            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void CreateCommand_To_Entity_IgnoresOfficerSalesGroups()
        {
            var command = new CreateMarketingOfficerCommand
            {
                EmployeeNo = "EMP001",
                EmployeeName = "John Doe",
                SalesOfficeId = 1,
                SalesGroups = new List<CreateOfficerSalesGroupDto>
                {
                    new CreateOfficerSalesGroupDto { SalesGroupId = 1 }
                }
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.MarketingOfficer>(command);

            entity.OfficerSalesGroups.Should().BeNull();
        }

        [Fact]
        public void UpdateCommand_IsActive1_MapsTo_StatusActive()
        {
            var command = new UpdateMarketingOfficerCommand
            {
                Id = 1,
                EmployeeName = "Updated",
                SalesOfficeId = 1,
                IsActive = 1
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.MarketingOfficer>(command);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void UpdateCommand_IsActive0_MapsTo_StatusInactive()
        {
            var command = new UpdateMarketingOfficerCommand
            {
                Id = 1,
                EmployeeName = "Updated",
                SalesOfficeId = 1,
                IsActive = 0
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.MarketingOfficer>(command);

            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void UpdateCommand_To_Entity_IgnoresOfficerSalesGroups()
        {
            var command = new UpdateMarketingOfficerCommand
            {
                Id = 1,
                EmployeeName = "Updated",
                SalesOfficeId = 1,
                IsActive = 1,
                SalesGroups = new List<UpdateOfficerSalesGroupDto>
                {
                    new UpdateOfficerSalesGroupDto { SalesGroupId = 1 }
                }
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.MarketingOfficer>(command);

            entity.OfficerSalesGroups.Should().BeNull();
        }

        [Fact]
        public void UpdateCommand_To_Entity_MapsFields()
        {
            var command = new UpdateMarketingOfficerCommand
            {
                Id = 5,
                EmployeeName = "Updated Name",
                MobileNo = "1234567890",
                Email = "updated@test.com",
                Unit = "Unit B",
                Department = "Marketing",
                Designation = "Manager",
                SalesOfficeId = 2,
                IsActive = 1
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.MarketingOfficer>(command);

            entity.Id.Should().Be(5);
            entity.EmployeeName.Should().Be("Updated Name");
            entity.MobileNo.Should().Be("1234567890");
            entity.Email.Should().Be("updated@test.com");
            entity.SalesOfficeId.Should().Be(2);
        }
    }
}
