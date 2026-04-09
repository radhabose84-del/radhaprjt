using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ICustomerVisit;
using SalesManagement.Application.CustomerVisit.Dto;
using SalesManagement.Application.CustomerVisit.Queries.GetCustomerVisitAutoComplete;

namespace SalesManagement.UnitTests.Application.CustomerVisit.Queries
{
    public class GetCustomerVisitAutoCompleteQueryHandlerTests
    {
        private readonly Mock<ICustomerVisitQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly Mock<IMediator> _mockMediator = new();

        private GetCustomerVisitAutoCompleteQueryHandler CreateSut()
        {
            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return new GetCustomerVisitAutoCompleteQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
        }

        [Fact]
        public async Task Handle_ReturnsResults_WhenDataExists()
        {
            var data = new List<CustomerVisitLookupDto>
            {
                new() { Id = 1, CustomerId = 10, CustomerName = "Customer A" }
            } as IReadOnlyList<CustomerVisitLookupDto>;

            _mockQueryRepo.Setup(r => r.AutocompleteAsync("test", It.IsAny<CancellationToken>()))
                .ReturnsAsync(data);
            _mockMapper
                .Setup(m => m.Map<List<CustomerVisitLookupDto>>(data))
                .Returns(data.ToList());

            var result = await CreateSut().Handle(
                new GetCustomerVisitAutoCompleteQuery("test"),
                CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyTerm_UsesEmptyString()
        {
            var data = new List<CustomerVisitLookupDto>() as IReadOnlyList<CustomerVisitLookupDto>;
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("", It.IsAny<CancellationToken>()))
                .ReturnsAsync(data);
            _mockMapper
                .Setup(m => m.Map<List<CustomerVisitLookupDto>>(data))
                .Returns(new List<CustomerVisitLookupDto>());

            var result = await CreateSut().Handle(
                new GetCustomerVisitAutoCompleteQuery(""),
                CancellationToken.None);

            result.Should().BeEmpty();
            _mockQueryRepo.Verify(r => r.AutocompleteAsync("", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            var data = new List<CustomerVisitLookupDto>() as IReadOnlyList<CustomerVisitLookupDto>;
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("xyz", It.IsAny<CancellationToken>()))
                .ReturnsAsync(data);
            _mockMapper
                .Setup(m => m.Map<List<CustomerVisitLookupDto>>(data))
                .Returns(new List<CustomerVisitLookupDto>());

            var result = await CreateSut().Handle(
                new GetCustomerVisitAutoCompleteQuery("xyz"),
                CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
