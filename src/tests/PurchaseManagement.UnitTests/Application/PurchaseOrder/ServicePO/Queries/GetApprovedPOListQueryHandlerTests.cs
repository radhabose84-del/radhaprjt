using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ServicePO;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetApprovedPOList;

namespace PurchaseManagement.UnitTests.Application.PurchaseOrder.ServicePO.Queries
{
    public sealed class GetApprovedPOListQueryHandlerTests
    {
        private readonly Mock<IServicePurchaseOrderQueryRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetApprovedPOListQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockRepo
                .Setup(r => r.GetApprovedServicePoAsync())
                .ReturnsAsync(new List<PoIdNumberDto>());

            var result = await CreateSut().Handle(new GetApprovedPOListQuery(), CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            _mockRepo
                .Setup(r => r.GetApprovedServicePoAsync())
                .ReturnsAsync(new List<PoIdNumberDto>());

            await CreateSut().Handle(new GetApprovedPOListQuery(), CancellationToken.None);

            _mockRepo.Verify(r => r.GetApprovedServicePoAsync(), Times.Once);
        }
    }
}
