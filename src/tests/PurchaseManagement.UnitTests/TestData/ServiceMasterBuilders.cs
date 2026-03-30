using PurchaseManagement.Application.ServiceMaster.Commands.CreateService;
using PurchaseManagement.Application.ServiceMaster.Commands.DeleteService;
using PurchaseManagement.Application.ServiceMaster.Commands.UpdateService;
using PurchaseManagement.Application.ServiceMaster.Queries.GetAllServices;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.TestData
{
    public static class ServiceMasterBuilders
    {
        public static CreateServiceCommand ValidCreateCommand(
            string description = "Test Service",
            int sacId = 1,
            int uomId = 1,
            int? serviceCategoryId = 1,
            byte isActive = 1) =>
            new CreateServiceCommand
            {
                ServiceDescription = description,
                SacId = sacId,
                UomId = uomId,
                ServiceCategoryId = serviceCategoryId,
                IsActive = isActive
            };

        public static UpdateServiceCommand ValidUpdateCommand(
            int id = 1,
            string description = "Updated Service",
            int sacId = 1,
            int uomId = 1,
            int serviceCategoryId = 1,
            byte isActive = 1) =>
            new UpdateServiceCommand
            {
                Id = id,
                ServiceDescription = description,
                SacId = sacId,
                UomId = uomId,
                ServiceCategoryId = serviceCategoryId,
                IsActive = isActive
            };

        public static DeleteServiceCommand ValidDeleteCommand(int id = 1) =>
            new DeleteServiceCommand { Id = id };

        public static GetServiceMasterDto ValidDto(int id = 1, string serviceCode = "SRV001") =>
            new GetServiceMasterDto
            {
                Id = id,
                ServiceCode = serviceCode,
                ServiceDescription = "Test Service",
                SacId = 1,
                SacName = "Test SAC",
                UomId = 1,
                UomName = "Nos",
                ServiceCategoryId = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        public static PurchaseManagement.Domain.Entities.ServiceMaster ValidEntity(int id = 1) =>
            new PurchaseManagement.Domain.Entities.ServiceMaster
            {
                Id = id,
                ServiceCode = "SRV001",
                ServiceDescription = "Test Service",
                SacId = 1,
                UomId = 1,
                ServiceCategoryId = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
