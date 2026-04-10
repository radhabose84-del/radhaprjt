using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchase.DutyMaster;
using PurchaseManagement.Application.DutyMaster;
using PurchaseManagement.Application.DutyMaster.Queries.GetAllDutyMaster;
using PurchaseManagement.Application.Purchase.DutyMaster.GetAllDutyMaster;

namespace PurchaseManagement.UnitTests.Application.DutyMaster.Queries
{
    public sealed class GetAllDutyMasterQueryHandlerTests
    {
        private readonly Mock<IDutyMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetDutyMastersPagedQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupEmptyResult()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(((IReadOnlyList<PurchaseManagement.Domain.Entities.DutyMaster>)new List<PurchaseManagement.Domain.Entities.DutyMaster>(), 0));
            _mockMapper
                .Setup(m => m.Map<DutyMasterDto>(It.IsAny<PurchaseManagement.Domain.Entities.DutyMaster>()))
                .Returns(new DutyMasterDto());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            SetupEmptyResult();

            var (items, total) = await CreateSut().Handle(new GetAllDutyMasterQuery(1, 20), CancellationToken.None);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task Handle_WithRecords_ReturnsMappedDtos()
        {
            var entities = new List<PurchaseManagement.Domain.Entities.DutyMaster>
            {
                new() { Id = 1, DutyCode = "DC001" },
                new() { Id = 2, DutyCode = "DC002" }
            };
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(((IReadOnlyList<PurchaseManagement.Domain.Entities.DutyMaster>)entities, 2));
            _mockMapper
                .Setup(m => m.Map<DutyMasterDto>(It.IsAny<PurchaseManagement.Domain.Entities.DutyMaster>()))
                .Returns(new DutyMasterDto());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var (items, total) = await CreateSut().Handle(new GetAllDutyMasterQuery(1, 20), CancellationToken.None);

            items.Should().HaveCount(2);
            total.Should().Be(2);
        }

        [Fact]
        public async Task Handle_CallsGetAllAsyncOnce()
        {
            SetupEmptyResult();

            await CreateSut().Handle(new GetAllDutyMasterQuery(1, 20, "search"), CancellationToken.None);

            _mockQueryRepo.Verify(
                r => r.GetAllAsync(1, 20, "search", It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            SetupEmptyResult();

            await CreateSut().Handle(new GetAllDutyMasterQuery(1, 20), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
