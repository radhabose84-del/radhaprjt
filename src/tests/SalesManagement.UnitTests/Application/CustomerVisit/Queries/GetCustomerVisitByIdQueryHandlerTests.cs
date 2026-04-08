using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ICustomerVisit;
using SalesManagement.Application.CustomerVisit.Dto;
using SalesManagement.Application.CustomerVisit.Queries.GetCustomerVisitById;

namespace SalesManagement.UnitTests.Application.CustomerVisit.Queries
{
    public class GetCustomerVisitByIdQueryHandlerTests
    {
        private readonly Mock<ICustomerVisitQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly Mock<IMediator> _mockMediator = new();

        private GetCustomerVisitByIdQueryHandler CreateSut()
        {
            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return new GetCustomerVisitByIdQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
        }

        [Fact]
        public async Task Handle_ExistingId_ReturnsMappedDto()
        {
            var dto = new CustomerVisitDto { Id = 1, CustomerId = 10, VisitTypeId = 2 };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);
            _mockMapper.Setup(m => m.Map<CustomerVisitDto>(dto)).Returns(dto);

            var result = await CreateSut().Handle(
                new GetCustomerVisitByIdQuery { Id = 1 },
                CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NonExistentId_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((CustomerVisitDto?)null);

            var result = await CreateSut().Handle(
                new GetCustomerVisitByIdQuery { Id = 99 },
                CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_ExistingId_CallsGetByIdAsync_Once()
        {
            var dto = new CustomerVisitDto { Id = 1 };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);
            _mockMapper.Setup(m => m.Map<CustomerVisitDto>(dto)).Returns(dto);

            await CreateSut().Handle(
                new GetCustomerVisitByIdQuery { Id = 1 },
                CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetByIdAsync(1), Times.Once);
        }
    }
}
