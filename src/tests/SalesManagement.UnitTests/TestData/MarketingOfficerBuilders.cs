using SalesManagement.Application.MarketingOfficer.Commands.CreateMarketingOfficer;
using SalesManagement.Application.MarketingOfficer.Commands.UpdateMarketingOfficer;
using SalesManagement.Application.MarketingOfficer.Dto;
using SalesManagement.Domain.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.TestData
{
    public static class MarketingOfficerBuilders
    {
        public static CreateMarketingOfficerCommand ValidCreateCommand(
            string employeeNo = "EMP001",
            string employeeName = "Test Officer",
            string? mobileNo = "9876543210",
            string? email = "test@example.com",
            string unit = "Unit A",
            string department = "Sales Dept",
            string designation = "Manager",
            int salesOfficeId = 1,
            List<CreateOfficerSalesGroupDto>? salesGroups = null) =>
            new CreateMarketingOfficerCommand
            {
                EmployeeNo = employeeNo,
                EmployeeName = employeeName,
                MobileNo = mobileNo,
                Email = email,
                Unit = unit,
                Department = department,
                Designation = designation,
                SalesOfficeId = salesOfficeId,
                SalesGroups = salesGroups ?? new List<CreateOfficerSalesGroupDto>
                {
                    new CreateOfficerSalesGroupDto { SalesGroupId = 1 }
                }
            };

        public static UpdateMarketingOfficerCommand ValidUpdateCommand(
            int id = 1,
            string employeeName = "Updated Officer",
            string? mobileNo = "9876543210",
            string? email = "updated@example.com",
            string unit = "Unit B",
            string department = "Marketing Dept",
            string designation = "Senior Manager",
            int salesOfficeId = 1,
            int isActive = 1,
            List<UpdateOfficerSalesGroupDto>? salesGroups = null) =>
            new UpdateMarketingOfficerCommand
            {
                Id = id,
                EmployeeName = employeeName,
                MobileNo = mobileNo,
                Email = email,
                Unit = unit,
                Department = department,
                Designation = designation,
                SalesOfficeId = salesOfficeId,
                IsActive = isActive,
                SalesGroups = salesGroups ?? new List<UpdateOfficerSalesGroupDto>
                {
                    new UpdateOfficerSalesGroupDto { SalesGroupId = 1 }
                }
            };

        public static MarketingOfficerDto ValidDto(
            int id = 1,
            string employeeNo = "EMP001",
            string employeeName = "Test Officer",
            int salesOfficeId = 1,
            string salesOfficeName = "Office A") =>
            new MarketingOfficerDto
            {
                Id = id,
                EmployeeNo = employeeNo,
                EmployeeName = employeeName,
                MobileNo = "9876543210",
                Email = "test@example.com",
                Unit = "Unit A",
                Department = "Sales Dept",
                Designation = "Manager",
                SalesOfficeId = salesOfficeId,
                SalesOfficeName = salesOfficeName,
                IsActive = true,
                IsDeleted = false,
                SalesGroups = new List<OfficerSalesGroupDto>
                {
                    new OfficerSalesGroupDto { Id = 1, SalesGroupId = 1, SalesGroupName = "Group A" }
                }
            };

        public static IReadOnlyList<MarketingOfficerLookupDto> ValidLookupList() =>
            new List<MarketingOfficerLookupDto>
            {
                new MarketingOfficerLookupDto { Id = 1, EmployeeNo = "EMP001", EmployeeName = "Test Officer" }
            };

        public static SalesManagement.Domain.Entities.MarketingOfficer ValidEntity(int id = 1) =>
            new SalesManagement.Domain.Entities.MarketingOfficer
            {
                Id = id,
                EmployeeNo = "EMP001",
                EmployeeName = "Test Officer",
                MobileNo = "9876543210",
                Email = "test@example.com",
                Unit = "Unit A",
                Department = "Sales Dept",
                Designation = "Manager",
                SalesOfficeId = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted,
                OfficerSalesGroups = new List<SalesManagement.Domain.Entities.OfficerSalesGroup>
                {
                    new SalesManagement.Domain.Entities.OfficerSalesGroup
                    {
                        Id = 1,
                        SalesGroupId = 1,
                        IsActive = Status.Active,
                        IsDeleted = IsDelete.NotDeleted
                    }
                }
            };
    }
}
