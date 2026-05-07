using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ServicePO;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetScheduleByPoIdandSeviceidandServiceItemid;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.UnitTests.Application.PurchaseOrder.ServicePO.Queries
{
    public sealed class GetSESCreateSourceQueryHandlerTests
    {
        private readonly Mock<IServicePurchaseOrderQueryRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetSESCreateSourceQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMediator.Object);

        private static GetSESCreateSourceQuery BuildQuery(
            int purchaseOrderId = 4,
            int scheduleNo = 1,
            int serviceItemId = 3) =>
            new()
            {
                PurchaseOrderId = purchaseOrderId,
                ScheduleNo = scheduleNo,
                ServiceItemId = serviceItemId
            };

        private static SesFromScheduleRawDto BuildDto(int scheduleId = 99) =>
            new()
            {
                ScheduleId = scheduleId,
                PurchaseOrderId = 4,
                ServiceId = 1,
                OccurrenceNo = 1,
                ScheduleStartDate = new DateTime(2026, 5, 5),
                ScheduleEndDate = new DateTime(2026, 5, 6)
            };

        [Fact]
        public async Task Handle_FoundRow_ReturnsDto()
        {
            var dto = BuildDto();
            _mockRepo
                .Setup(r => r.GetSesCreateSourceAsync(4, 1, 3, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            var result = await CreateSut().Handle(BuildQuery(), CancellationToken.None);

            result.Should().NotBeNull();
            result!.ScheduleId.Should().Be(99);
        }

        [Fact]
        public async Task Handle_NotFound_ReturnsNull()
        {
            _mockRepo
                .Setup(r => r.GetSesCreateSourceAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((SesFromScheduleRawDto?)null);

            var result = await CreateSut().Handle(BuildQuery(), CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_PassesParametersToRepository()
        {
            _mockRepo
                .Setup(r => r.GetSesCreateSourceAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(BuildDto());

            await CreateSut().Handle(BuildQuery(2686, 1, 1109), CancellationToken.None);

            _mockRepo.Verify(
                r => r.GetSesCreateSourceAsync(2686, 1, 1109, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_FoundRow_PublishesAuditEvent()
        {
            _mockRepo
                .Setup(r => r.GetSesCreateSourceAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(BuildDto(scheduleId: 42));

            await CreateSut().Handle(BuildQuery(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "GetSesCreateSource" &&
                        e.ActionCode == "GetSESCreateSourceQuery" &&
                        e.ActionName == "42"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NotFound_StillPublishesAuditEvent_WithZeroActionName()
        {
            _mockRepo
                .Setup(r => r.GetSesCreateSourceAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((SesFromScheduleRawDto?)null);

            await CreateSut().Handle(BuildQuery(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionName == "0"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public void QueryClass_Properties_ShouldBeAssignable()
        {
            var query = new GetSESCreateSourceQuery
            {
                PurchaseOrderId = 4,
                ScheduleNo = 1,
                ServiceItemId = 3
            };
            query.PurchaseOrderId.Should().Be(4);
            query.ScheduleNo.Should().Be(1);
            query.ServiceItemId.Should().Be(3);
        }
    }
}
