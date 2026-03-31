using AutoMapper;
using FAM.Application.AssetMaster.AssetInsurance.Commands.CreateAssetInsurance;
using FAM.Application.AssetMaster.AssetInsurance.Commands.DeleteAssetInsurance;
using FAM.Application.AssetMaster.AssetInsurance.Commands.UpdateAssetInsurance;
using FAM.Application.Common.Mappings.AssetMaster;
using static FAM.Domain.Common.BaseEntity;

namespace FixedAssetManagement.UnitTests.Mappings
{
    public sealed class AssetInsuranceProfileTests
    {
        private readonly IMapper _mapper;

        public AssetInsuranceProfileTests()
        {
            var config = new MapperConfiguration(cfg =>
                cfg.AddProfile<AssetInsuranceProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void CreateCommand_IsActive1_MapsTo_StatusActive()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var cmd = new CreateAssetInsuranceCommand
            {
                AssetId = 1,
                PolicyNo = "POL001",
                StartDate = today,
                Insuranceperiod = 12,
                EndDate = today.AddMonths(12),
                PolicyAmount = 5000m,
                VendorCode = "VND001",
                RenewalStatus = 1,
                RenewedDate = today,
                IsActive = 1
            };

            var entity = _mapper.Map<FAM.Domain.Entities.AssetMaster.AssetInsurance>(cmd);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void CreateCommand_IsActive0_MapsTo_StatusInactive()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var cmd = new CreateAssetInsuranceCommand
            {
                AssetId = 1,
                PolicyNo = "POL001",
                StartDate = today,
                EndDate = today.AddMonths(12),
                RenewedDate = today,
                IsActive = 0
            };

            var entity = _mapper.Map<FAM.Domain.Entities.AssetMaster.AssetInsurance>(cmd);

            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsDeleted_NotDeleted()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var cmd = new CreateAssetInsuranceCommand
            {
                AssetId = 1,
                StartDate = today,
                EndDate = today.AddMonths(12),
                RenewedDate = today,
                IsActive = 1
            };

            var entity = _mapper.Map<FAM.Domain.Entities.AssetMaster.AssetInsurance>(cmd);

            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void DeleteCommand_To_Entity_SetsIsDeleted_Deleted()
        {
            var cmd = new DeleteAssetInsuranceCommand { Id = 5 };

            var entity = _mapper.Map<FAM.Domain.Entities.AssetMaster.AssetInsurance>(cmd);

            entity.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public void UpdateCommand_IsActive1_MapsTo_StatusActive()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var cmd = new UpdateAssetInsuranceCommand
            {
                PolicyNo = "POL002",
                StartDate = today,
                EndDate = today.AddMonths(12),
                RenewedDate = today,
                IsActive = 1
            };

            var entity = _mapper.Map<FAM.Domain.Entities.AssetMaster.AssetInsurance>(cmd);

            entity.IsActive.Should().Be(Status.Active);
        }
    }
}
