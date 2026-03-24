using PurchaseManagement.Application.PaymentTermMaster.Command.CreatePaymentTermMaster;
using PurchaseManagement.Application.PaymentTermMaster.Command.UpdatePaymentTermMaster;
using PurchaseManagement.Application.PaymentTermMaster.Command.DeletePaymentTermMaster;
using PurchaseManagement.Application.PaymentTermMaster.Queries.GetAllPaymentTermMaster;

namespace PurchaseManagement.UnitTests.TestData
{
    public static class PaymentTermMasterBuilders
    {
        public static CreatePaymentTermMasterCommand ValidCreateCommand(
            string code = "PT001",
            string description = "Test Payment Term",
            int baselineTypeId = 1) =>
            new CreatePaymentTermMasterCommand
            {
                Code = code,
                Description = description,
                BaselineTypeId = baselineTypeId,
                CreditDays = 30,
                AdvancePercent = 0m,
                DiscountPercent = null,
                DiscountDays = null,
                GraceDays = null,
                AdditionalValue = 0m,
                ApplicableForPortal = false,
                Installments = null
            };

        public static UpdatePaymentTermMasterCommand ValidUpdateCommand(
            int id = 1,
            string code = "PT001",
            string description = "Updated Payment Term",
            bool isActive = true) =>
            new UpdatePaymentTermMasterCommand
            {
                Id = id,
                Code = code,
                Description = description,
                BaselineTypeId = 1,
                CreditDays = 30,
                AdvancePercent = 0m,
                IsActive = isActive,
                Installments = null
            };

        public static DeletePaymentTermMasterCommand ValidDeleteCommand(int id = 1) =>
            new DeletePaymentTermMasterCommand { Id = id };

        public static PaymentTermMasterDto ValidDto(
            int id = 1,
            string code = "PT001",
            string description = "Test Payment Term") =>
            new PaymentTermMasterDto
            {
                Id = id,
                Code = code,
                Description = description,
                BaselineTypeId = 1,
                CreditDays = 30,
                IsActive = true
            };

        public static PurchaseManagement.Domain.Entities.PaymentTermMaster ValidEntity(int id = 1) =>
            new PurchaseManagement.Domain.Entities.PaymentTermMaster
            {
                Id = id,
                Code = "PT001",
                Description = "Test Payment Term",
                BaselineTypeId = 1,
                CreditDays = 30,
                IsActive = PurchaseManagement.Domain.Common.BaseEntity.Status.Active,
                IsDeleted = PurchaseManagement.Domain.Common.BaseEntity.IsDelete.NotDeleted
            };
    }
}
