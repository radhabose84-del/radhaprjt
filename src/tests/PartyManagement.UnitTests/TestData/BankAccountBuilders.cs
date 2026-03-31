using PartyManagement.Application.BankAccount.Command.CreateBankAccount;
using PartyManagement.Application.BankAccount.Command.UpdateBankAccount;
using PartyManagement.Application.BankAccount.Command.DeleteBankAccount;
using PartyManagement.Domain.Common;

namespace PartyManagement.UnitTests.TestData
{
    public static class BankAccountBuilders
    {
        public static CreateBankAccountCommand ValidCreateCommand(int bankId = 1) =>
            new CreateBankAccountCommand(
                BankId: bankId,
                AccountNumber: "1234567890",
                AccountHolderName: "Test Holder",
                BranchId: 1,
                IFSCCode: "ICIC0001234",
                SWIFTCode: null,
                AccountTypeId: 1,
                IsDefaultAccount: true,
                IsPrimaryAccount: true,
                IBan: null
            );

        public static UpdateBankAccountCommand ValidUpdateCommand(int id = 1) =>
            new UpdateBankAccountCommand(
                Id: id,
                BankId: 1,
                AccountNumber: "9876543210",
                AccountHolderName: "Updated Holder",
                BranchId: 1,
                IFSCCode: "HDFC0001234",
                SWIFTCode: null,
                AccountTypeId: 1,
                IsDefaultAccount: true,
                IsPrimaryAccount: false,
                IBan: null,
                IsActive: BaseEntity.Status.Active
            );

        public static DeleteBankAccountCommand ValidDeleteCommand(int id = 1) =>
            new DeleteBankAccountCommand(id);

        public static PartyManagement.Domain.Entities.BankAccount ValidEntity(int id = 1) =>
            new PartyManagement.Domain.Entities.BankAccount
            {
                Id = id,
                BankId = 1,
                AccountNumber = "1234567890",
                AccountHolderName = "Test Holder",
                BranchId = 1,
                IFSCCode = "ICIC0001234",
                AccountTypeId = 1,
                IsDefaultAccount = true,
                IsPrimaryAccount = true,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
    }
}
