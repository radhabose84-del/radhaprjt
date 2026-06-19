using FinanceManagement.Application.VoucherType.Commands.CreateVoucherType;
using FinanceManagement.Application.VoucherType.Commands.UpdateVoucherType;
using FinanceManagement.Application.VoucherType.Dto;

namespace FinanceManagement.UnitTests.TestData
{
    internal static class VoucherTypeBuilders
    {
        public static CreateVoucherTypeCommand ValidCreateCommand(
            string code = "JV",
            string name = "Journal Voucher",
            int numberPadding = 4,
            List<int>? accountTypeIds = null) =>
            new()
            {
                VoucherTypeCode = code,
                VoucherTypeName = name,
                NumberPadding = numberPadding,
                AllowedAccountTypeIds = accountTypeIds ?? new List<int> { 1, 2, 3, 4 }
            };

        public static UpdateVoucherTypeCommand ValidUpdateCommand(
            int id = 1,
            string name = "Journal Voucher",
            int numberPadding = 4,
            int isActive = 1,
            List<int>? accountTypeIds = null) =>
            new()
            {
                Id = id,
                VoucherTypeName = name,
                NumberPadding = numberPadding,
                IsActive = isActive,
                AllowedAccountTypeIds = accountTypeIds ?? new List<int> { 1, 2 }
            };

        public static VoucherTypeMasterDto ValidDto(
            int id = 1,
            string code = "JV",
            string name = "Journal Voucher") =>
            new()
            {
                Id = id,
                CompanyId = 1,
                CompanyName = "Test Company",
                VoucherTypeCode = code,
                VoucherTypeName = name,
                NumberPadding = 4,
                IsSystem = false,
                FinancialYearId = 3,
                FinancialYearName = "2026-27",
                LastUsedNumber = 427,
                NextNumber = "JV/2026-27/0428",
                IsActive = true,
                IsDeleted = false,
                AllowedAccountTypes = new List<VoucherTypeAccountTypeDto>
                {
                    new() { AccountTypeId = 1, AccountTypeName = "Asset" }
                }
            };

        public static List<VoucherTypeLookupDto> ValidLookupList() =>
            new()
            {
                new VoucherTypeLookupDto { Id = 1, VoucherTypeCode = "JV", VoucherTypeName = "Journal Voucher" }
            };

        public static List<VoucherTypeNumberSeriesDto> ValidNumberSeriesList() =>
            new()
            {
                new VoucherTypeNumberSeriesDto
                {
                    VoucherTypeId = 1, VoucherTypeCode = "JV", VoucherTypeName = "Journal Voucher",
                    NumberPadding = 4, FinancialYearId = 3, FinancialYearName = "2026-27",
                    LastUsedNumber = 427, NextNumberValue = 428, NextNumber = "JV/2026-27/0428"
                }
            };

        public static FinanceManagement.Domain.Entities.VoucherTypeMaster ValidEntity(int id = 1) =>
            new()
            {
                Id = id,
                CompanyId = 1,
                VoucherTypeCode = "JV",
                VoucherTypeName = "Journal Voucher",
                NumberPadding = 4,
                IsSystem = false,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
