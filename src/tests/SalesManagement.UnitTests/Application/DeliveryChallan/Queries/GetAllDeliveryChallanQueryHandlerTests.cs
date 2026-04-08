using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDeliveryChallan;
using SalesManagement.Application.DeliveryChallan.Dto;
using SalesManagement.Application.DeliveryChallan.Queries.GetAllDeliveryChallan;

namespace SalesManagement.UnitTests.Application.DeliveryChallan.Queries
{
    public sealed class GetAllDeliveryChallanQueryHandlerTests
    {
        private readonly Mock<IDeliveryChallanQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAllDeliveryChallanQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((new List<DeliveryChallanHeaderDto> { new DeliveryChallanHeaderDto() }, 1));

            var result = await CreateSut().Handle(
                new GetAllDeliveryChallanQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((new List<DeliveryChallanHeaderDto>(), 0));

            var result = await CreateSut().Handle(
                new GetAllDeliveryChallanQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(2, 5, "search"))
                .ReturnsAsync((new List<DeliveryChallanHeaderDto>(), 11));

            var result = await CreateSut().Handle(
                new GetAllDeliveryChallanQuery { PageNumber = 2, PageSize = 5, SearchTerm = "search" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }
    }
}
