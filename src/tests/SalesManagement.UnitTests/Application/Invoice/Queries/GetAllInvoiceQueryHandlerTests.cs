using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IInvoice;
using SalesManagement.Application.Invoice.Dto;
using SalesManagement.Application.Invoice.Queries.GetAllInvoice;

namespace SalesManagement.UnitTests.Application.Invoice.Queries
{
    public sealed class GetAllInvoiceQueryHandlerTests
    {
        private readonly Mock<IInvoiceQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAllInvoiceQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null, null))
                .ReturnsAsync((new List<InvoiceHeaderDto> { new() }, 1));

            var result = await CreateSut().Handle(
                new GetAllInvoiceQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null, null))
                .ReturnsAsync((new List<InvoiceHeaderDto>(), 0));

            var result = await CreateSut().Handle(
                new GetAllInvoiceQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_WithStatusFilter_PassesStatus()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null, "Pending"))
                .ReturnsAsync((new List<InvoiceHeaderDto>(), 0));

            var result = await CreateSut().Handle(
                new GetAllInvoiceQuery { PageNumber = 1, PageSize = 10, Status = "Pending" }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }
    }
}
