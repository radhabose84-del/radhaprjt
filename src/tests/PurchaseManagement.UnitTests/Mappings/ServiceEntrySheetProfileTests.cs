using AutoMapper;
using PurchaseManagement.Application.Common.Mappings.PurchaseOrder;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.CreateServiceEntrySheet;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetScheduleByPoIdandSeviceidandServiceItemid;

namespace PurchaseManagement.UnitTests.Mappings
{
    public sealed class ServiceEntrySheetProfileTests
    {
        private readonly IMapper _mapper;
        private static readonly TimeSpan IstOffset = TimeSpan.FromHours(5.5);

        public ServiceEntrySheetProfileTests()
        {
            var config = new MapperConfiguration(cfg =>
                cfg.AddProfile<ServiceEntrySheetProfile>());
            _mapper = config.CreateMapper();
        }

        private static SesFromScheduleRawDto BuildRaw(
            DateTime? start = null,
            DateTime? end = null) =>
            new()
            {
                ScheduleId = 99,
                PurchaseOrderId = 4,
                UnitId = 1,
                VendorId = 36,
                ServiceId = 1,
                OccurrenceNo = 1,
                ScheduleStartDate = start,
                ScheduleEndDate = end
            };

        [Fact]
        public void Map_NaiveStartDate_WrapsWithIstOffset()
        {
            var src = BuildRaw(start: new DateTime(2026, 5, 5, 0, 0, 0));

            var dest = _mapper.Map<CreateServiceSheetDto>(src);

            dest.ScheduleStartDate.Should().Be(
                new DateTimeOffset(2026, 5, 5, 0, 0, 0, IstOffset));
        }

        [Fact]
        public void Map_NaiveEndDate_WrapsWithIstOffset()
        {
            var src = BuildRaw(end: new DateTime(2026, 5, 6, 0, 0, 0));

            var dest = _mapper.Map<CreateServiceSheetDto>(src);

            dest.ScheduleEndDate.Should().Be(
                new DateTimeOffset(2026, 5, 6, 0, 0, 0, IstOffset));
        }

        [Fact]
        public void Map_NullStartDate_ProducesDefaultDateTimeOffset()
        {
            var src = BuildRaw(start: null);

            var dest = _mapper.Map<CreateServiceSheetDto>(src);

            dest.ScheduleStartDate.Should().Be(default(DateTimeOffset));
        }

        [Fact]
        public void Map_NullEndDate_ProducesDefaultDateTimeOffset()
        {
            var src = BuildRaw(end: null);

            var dest = _mapper.Map<CreateServiceSheetDto>(src);

            dest.ScheduleEndDate.Should().Be(default(DateTimeOffset));
        }

        [Fact]
        public void AfterMap_WorkStartDate_FallsBackToScheduleStartDate()
        {
            var src = BuildRaw(start: new DateTime(2026, 5, 5));

            var dest = _mapper.Map<CreateServiceSheetDto>(src);

            dest.WorkStartDate.Should().Be(dest.ScheduleStartDate);
        }

        [Fact]
        public void AfterMap_WorkEndDate_FallsBackToScheduleEndDate()
        {
            var src = BuildRaw(end: new DateTime(2026, 5, 6));

            var dest = _mapper.Map<CreateServiceSheetDto>(src);

            dest.WorkEndDate.Should().Be(dest.ScheduleEndDate);
        }

        [Fact]
        public void Map_SesStatusId_DefaultsToDraft_1103()
        {
            var dest = _mapper.Map<CreateServiceSheetDto>(BuildRaw());

            dest.SESStatusId.Should().Be(1103);
        }

        [Fact]
        public void Map_ScheduleId_FlowsToScheduleID()
        {
            var src = BuildRaw();
            src.ScheduleId = 777;

            var dest = _mapper.Map<CreateServiceSheetDto>(src);

            dest.ScheduleID.Should().Be(777);
        }

        [Fact]
        public void Map_GstPercent_FlowsToTaxPercentage()
        {
            var src = BuildRaw();
            src.GstPercent = 18.0m;

            var dest = _mapper.Map<CreateServiceSheetDto>(src);

            dest.TaxPercentage.Should().Be(18.0m);
        }
    }
}
