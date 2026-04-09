using AutoMapper;
using PurchaseManagement.Application.PriceMaster.Command.CreatePriceMaster;
using PurchaseManagement.Application.PriceMaster.Dtos;
using PurchaseManagement.Application.PriceMaster.Mapping;
using PurchaseManagement.Domain.Entities.PriceMaster;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Mappings
{
    public sealed class PriceMasterProfileTests
    {
        private readonly IMapper _mapper;

        public PriceMasterProfileTests()
        {
            var config = new MapperConfiguration(cfg =>
                cfg.AddProfile<PriceMasterProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void CreateDto_To_Header_MapsItemId()
        {
            var dto = new PriceMasterCreateDto
            {
                ItemId = 10,
                VendorId = 20,
                UomId = 5,
                ValidFrom = new DateOnly(2025, 1, 1)
            };

            var entity = _mapper.Map<PriceMasterHeader>(dto);

            entity.ItemId.Should().Be(10);
            entity.VendorId.Should().Be(20);
            entity.UomId.Should().Be(5);
        }

        [Fact]
        public void CreateDto_To_Header_IgnoresDetails()
        {
            var dto = new PriceMasterCreateDto
            {
                ItemId = 1,
                VendorId = 1,
                UomId = 1,
                ValidFrom = new DateOnly(2025, 1, 1),
                Details = new List<PriceMasterDetailUpsertDto>
                {
                    new() { ScaleQtyFrom = 1, UnitPrice = 100m, CurrencyId = 1 }
                }
            };

            var entity = _mapper.Map<PriceMasterHeader>(dto);

            entity.Details.Should().BeEmpty();
        }

        [Fact]
        public void UpdateDto_To_Header_IgnoresId()
        {
            var dto = new PriceMasterUpdateDto
            {
                Id = 99,
                ItemId = 10,
                VendorId = 20,
                UomId = 5,
                ValidFrom = new DateOnly(2025, 6, 1)
            };

            var entity = _mapper.Map<PriceMasterHeader>(dto);

            entity.Id.Should().Be(0); // Id is ignored
            entity.ItemId.Should().Be(10);
        }

        [Fact]
        public void DetailUpsertDto_To_Detail_MapsAllFields()
        {
            var dto = new PriceMasterDetailUpsertDto
            {
                ScaleQtyFrom = 1m,
                ScaleQtyTo = 100m,
                UnitPrice = 50.5m,
                CurrencyId = 2
            };

            var entity = _mapper.Map<PriceMasterDetail>(dto);

            entity.ScaleQtyFrom.Should().Be(1m);
            entity.ScaleQtyTo.Should().Be(100m);
            entity.UnitPrice.Should().Be(50.5m);
            entity.CurrencyId.Should().Be(2);
        }

        [Fact]
        public void Header_To_ReverseDto_MapsHeaderFields()
        {
            var header = PriceMasterBuilders.ValidHeader(5);
            header.Details = new List<PriceMasterDetail>();

            var reverseDto = _mapper.Map<CreatePriceMasterReverseDto>(header);

            reverseDto.Header.Should().NotBeNull();
            reverseDto.Header!.Id.Should().Be(5);
            reverseDto.Header.VendorId.Should().Be(header.VendorId);
        }

        [Fact]
        public void Header_To_ReverseDto_MapsLinesToDetails()
        {
            var header = PriceMasterBuilders.ValidHeader(1);
            var detail = PriceMasterBuilders.ValidDetail(1);
            detail.Id = 10;
            header.Details = new List<PriceMasterDetail> { detail };

            var reverseDto = _mapper.Map<CreatePriceMasterReverseDto>(header);

            reverseDto.Lines.Should().HaveCount(1);
            reverseDto.Lines!.First().Id.Should().Be(10);
        }
    }
}
