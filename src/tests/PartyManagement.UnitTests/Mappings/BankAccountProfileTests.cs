using AutoMapper;
using PartyManagement.Application.BankAccount;
using PartyManagement.Application.Common.Mappings;
using static PartyManagement.Domain.Common.BaseEntity;

namespace PartyManagement.UnitTests.Mappings
{
    public sealed class BankAccountProfileTests
    {
        private readonly IMapper _mapper;

        public BankAccountProfileTests()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<BankAccountProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void Map_BankAccount_To_BankAccountDto_ShouldMapCoreFields()
        {
            var entity = new PartyManagement.Domain.Entities.BankAccount
            {
                Id = 1,
                BankId = 10,
                AccountHolderName = "Test Holder",
                AccountNumber = "1234567890",
                BranchId = 5,
                IFSCCode = "HDFC0001234",
                SWIFTCode = "HDFCINBB",
                AccountTypeId = 2,
                IsDefaultAccount = true,
                IsPrimaryAccount = false,
                IBan = "IBAN001",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

            var dto = _mapper.Map<BankAccountDto>(entity);

            dto.Id.Should().Be(1);
            dto.BankId.Should().Be(10);
            dto.AccountHolderName.Should().Be("Test Holder");
            dto.AccountNumber.Should().Be("1234567890");
            dto.BranchId.Should().Be(5);
            dto.IFSCCode.Should().Be("HDFC0001234");
            dto.SWIFTCode.Should().Be("HDFCINBB");
            dto.AccountTypeId.Should().Be(2);
            dto.IsDefaultAccount.Should().BeTrue();
            dto.IsPrimaryAccount.Should().BeFalse();
            dto.IBan.Should().Be("IBAN001");
        }

        [Fact]
        public void Map_BankAccountDto_To_BankAccount_ShouldMapCoreFields()
        {
            var dto = new BankAccountDto
            {
                Id = 2,
                BankId = 20,
                AccountHolderName = "Reverse Holder",
                AccountNumber = "9876543210",
                BranchId = 3,
                IFSCCode = "ICIC0001111",
                SWIFTCode = null,
                AccountTypeId = 4,
                IsDefaultAccount = false,
                IsPrimaryAccount = true,
                IBan = null
            };

            var entity = _mapper.Map<PartyManagement.Domain.Entities.BankAccount>(dto);

            entity.Id.Should().Be(2);
            entity.BankId.Should().Be(20);
            entity.AccountHolderName.Should().Be("Reverse Holder");
            entity.AccountNumber.Should().Be("9876543210");
            entity.BranchId.Should().Be(3);
            entity.IFSCCode.Should().Be("ICIC0001111");
            entity.AccountTypeId.Should().Be(4);
            entity.IsDefaultAccount.Should().BeFalse();
            entity.IsPrimaryAccount.Should().BeTrue();
        }

        [Fact]
        public void Map_BankAccount_NullableFields_ShouldMapAsNull()
        {
            var entity = new PartyManagement.Domain.Entities.BankAccount
            {
                BankId = 1,
                AccountHolderName = "Holder",
                AccountNumber = "12345",
                BranchId = 1,
                AccountTypeId = 1,
                IFSCCode = null,
                SWIFTCode = null,
                IBan = null,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

            var dto = _mapper.Map<BankAccountDto>(entity);

            dto.IFSCCode.Should().BeNull();
            dto.SWIFTCode.Should().BeNull();
            dto.IBan.Should().BeNull();
        }

        [Fact]
        public void Map_BankAccount_BooleanFields_MapCorrectly()
        {
            var entity = new PartyManagement.Domain.Entities.BankAccount
            {
                BankId = 1,
                AccountHolderName = "Holder",
                AccountNumber = "12345",
                BranchId = 1,
                AccountTypeId = 1,
                IsDefaultAccount = true,
                IsPrimaryAccount = true,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

            var dto = _mapper.Map<BankAccountDto>(entity);

            dto.IsDefaultAccount.Should().BeTrue();
            dto.IsPrimaryAccount.Should().BeTrue();
        }
    }
}
