using AutoMapper;
using PartyManagement.Application.BankMaster;
using PartyManagement.Application.BankMaster.Mapping;
using static PartyManagement.Domain.Common.BaseEntity;
using Xunit;

namespace PartyManagement.UnitTests.Mappings
{
    public sealed class BankMasterProfileTests
    {
        private readonly IMapper _mapper;

        public BankMasterProfileTests()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<BankMasterProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void CreateDto_To_Entity_SetsIsActive_Active()
        {
            var dto = new CreateBankMasterDto("ICICI Bank");

            var entity = _mapper.Map<PartyManagement.Domain.Entities.BankMaster>(dto);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void CreateDto_To_Entity_SetsIsDeleted_NotDeleted()
        {
            var dto = new CreateBankMasterDto("ICICI Bank");

            var entity = _mapper.Map<PartyManagement.Domain.Entities.BankMaster>(dto);

            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void Entity_To_BankMasterDto_MapsIsActive_AsInt()
        {
            var entity = new PartyManagement.Domain.Entities.BankMaster
            {
                Id = 1,
                BankCode = "BNK001",
                BankName = "ICICI Bank",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

            var dto = _mapper.Map<BankMasterDto>(entity);

            dto.IsActive.Should().Be((int)Status.Active);
            dto.IsDeleted.Should().Be((int)IsDelete.NotDeleted);
        }

        [Fact]
        public void UpdateDto_To_Entity_SetsModifiedDate()
        {
            var dto = new UpdateBankMasterDto(1, "Updated Bank", 1);

            var entity = _mapper.Map<PartyManagement.Domain.Entities.BankMaster>(dto);

            entity.ModifiedDate.Should().NotBeNull();
        }
    }
}
