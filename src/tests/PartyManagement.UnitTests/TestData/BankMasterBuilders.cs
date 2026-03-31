using PartyManagement.Application.BankMaster;
using PartyManagement.Application.BankMaster.Command.Create;
using static PartyManagement.Domain.Common.BaseEntity;

namespace PartyManagement.UnitTests.TestData
{
    public static class BankMasterBuilders
    {
        public static CreateBankMasterCommand ValidCreateCommand(string bankName = "ICICI Bank") =>
            new CreateBankMasterCommand(new CreateBankMasterDto(bankName));

        public static BankMasterDto ValidDto(int id = 1, string bankCode = "BNK001", string bankName = "ICICI Bank") =>
            new BankMasterDto(id, bankCode, bankName, (int)Status.Active, (int)IsDelete.NotDeleted, DateTimeOffset.UtcNow);

        public static PartyManagement.Domain.Entities.BankMaster ValidEntity(int id = 1) =>
            new PartyManagement.Domain.Entities.BankMaster
            {
                Id = id,
                BankCode = "BNK001",
                BankName = "ICICI Bank",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
