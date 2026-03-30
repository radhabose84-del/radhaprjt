using PurchaseManagement.Application.Port.Commands;
using PurchaseManagement.Application.Port.Dto;
using PurchaseManagement.Application.Common.Interfaces.IPortMaster;
using DomainBase = PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.TestData
{
    public static class PortMasterBuilders
    {
        public static CreatePortMasterCommand ValidCreateCommand(
            string portCode = "PORT001",
            string portName = "Test Port",
            int countryId = 1,
            int portTypeId = 1) =>
            new CreatePortMasterCommand(portCode, portName, countryId, portTypeId);

        public static UpdatePortMasterCommand ValidUpdateCommand(
            int id = 1,
            string portCode = "PORT001",
            string portName = "Updated Port",
            int countryId = 1,
            int portTypeId = 1,
            int isActive = 1) =>
            new UpdatePortMasterCommand(id, portCode, portName, countryId, portTypeId, isActive);

        public static DeletePortMasterCommand ValidDeleteCommand(int id = 1) =>
            new DeletePortMasterCommand(id);

        public static PortMasterDto ValidDto(int id = 1) =>
            new PortMasterDto
            {
                Id = id,
                PortCode = "PORT001",
                PortName = "Test Port",
                CountryId = 1,
                PortTypeId = 1,
                IsActive = 1,
                Country = "India",
                PortType = "Sea Port"
            };

        public static PurchaseManagement.Domain.Entities.PortMaster ValidEntity(int id = 1) =>
            new PurchaseManagement.Domain.Entities.PortMaster
            {
                Id = id,
                PortCode = "PORT001",
                PortName = "Test Port",
                CountryId = 1,
                PortTypeId = 1,
                IsActive = DomainBase.Status.Active,
                IsDeleted = DomainBase.IsDelete.NotDeleted
            };

        public static PortLookupDto ValidLookupDto(int id = 1) =>
            new PortLookupDto
            {
                Id = id,
                portCode = "PORT001",
                portname = "Test Port"
            };
    }
}
