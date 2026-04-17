using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IProformaInvoice;
using SalesManagement.Application.ProformaInvoice.Dto;
using SalesManagement.Application.ProformaInvoice.Queries.GetProformaInvoiceBySalesOrder;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.ProformaInvoice.Queries
{
    public sealed class GetProformaInvoiceBySalesOrderQueryHandlerTests
    {
        private readonly Mock<IProformaInvoiceQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetProformaInvoiceBySalesOrderQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_Returns_Matching_List()
        {
            _mockQueryRepo.Setup(r => r.GetBySalesOrderIdAsync(7))
                .ReturnsAsync(new List<ProformaInvoiceDto>
                {
                    new() { Id = 1 }, new() { Id = 2 }
                });

            var result = await CreateSut().Handle(new GetProformaInvoiceBySalesOrderQuery { SalesOrderId = 7 }, CancellationToken.None);

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_Returns_Empty_When_NoMatch()
        {
            _mockQueryRepo.Setup(r => r.GetBySalesOrderIdAsync(99))
                .ReturnsAsync(new List<ProformaInvoiceDto>());

            var result = await CreateSut().Handle(new GetProformaInvoiceBySalesOrderQuery { SalesOrderId = 99 }, CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
