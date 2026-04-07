using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ICustomerVisit;
using SalesManagement.Application.CustomerVisit.Dto;
using SalesManagement.Application.CustomerVisit.Queries.GetAllCustomerVisit;

namespace SalesManagement.UnitTests.Application.CustomerVisit.Queries
{
    public class GetAllCustomerVisitQueryHandlerTests
    {
        private readonly Mock<ICustomerVisitQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly Mock<IMediator> _mockMediator = new();

        private GetAllCustomerVisitQueryHandler CreateSut()
        {
            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return new GetAllCustomerVisitQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
        }

        [Fact]
        public async Task Handle_ReturnsSuccess_WhenRepositoryReturnsData()
        {
            var data = new List<CustomerVisitDto>
            {
                new() { Id = 1, CustomerId = 10, VisitTypeId = 1 },
                new() { Id = 2, CustomerId = 11, VisitTypeId = 2 }
            };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((data, 2));

            var result = await CreateSut().Handle(
                new GetAllCustomerVisitQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ReturnsCorrectPaginationMetadata()
        {
            var data = new List<CustomerVisitDto> { new() { Id = 1 } };
            _mockQueryRepo.Setup(r => r.GetAllAsync(2, 5, "visit"))
                .ReturnsAsync((data, 11));

            var result = await CreateSut().Handle(
                new GetAllCustomerVisitQuery { PageNumber = 2, PageSize = 5, SearchTerm = "visit" },
                CancellationToken.None);

            result.TotalCount.Should().Be(11);
            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyListWithZeroCount()
        {
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((new List<CustomerVisitDto>(), 0));

            var result = await CreateSut().Handle(
                new GetAllCustomerVisitQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_CallsGetAllAsync_Once_WithCorrectParameters()
        {
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 20, "test"))
                .ReturnsAsync((new List<CustomerVisitDto>(), 0));

            await CreateSut().Handle(
                new GetAllCustomerVisitQuery { PageNumber = 1, PageSize = 20, SearchTerm = "test" },
                CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetAllAsync(1, 20, "test"), Times.Once);
        }
    }
}
