using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDeliveryChallan;
using SalesManagement.Application.DeliveryChallan.Dto;
using SalesManagement.Application.DeliveryChallan.Queries.GetPendingDeliveryChallan;

namespace SalesManagement.UnitTests.Application.DeliveryChallan.Queries
{
    public sealed class GetPendingDeliveryChallanQueryHandlerTests
    {
        private readonly Mock<IDeliveryChallanQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetPendingDeliveryChallanQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            _mockQueryRepo
                .Setup(r => r.GetPendingAsync(1, 10, null))
                .ReturnsAsync((new List<DeliveryChallanHeaderDto>(), 0));

            var result = await CreateSut().Handle(
                new GetPendingDeliveryChallanQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }
    }
}
