using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IProformaInvoice;
using SalesManagement.Application.ProformaInvoice.Dto;
using SalesManagement.Application.ProformaInvoice.Queries.GetAllProformaInvoice;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.ProformaInvoice.Queries
{
    public sealed class GetAllProformaInvoiceQueryHandlerTests
    {
        private readonly Mock<IProformaInvoiceQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAllProformaInvoiceQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((new List<ProformaInvoiceDto>(), 0));

            var result = await CreateSut().Handle(new GetAllProformaInvoiceQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_PropagatesPagination()
        {
            _mockQueryRepo.Setup(r => r.GetAllAsync(2, 5, "x"))
                .ReturnsAsync((new List<ProformaInvoiceDto> { new() }, 23));

            var result = await CreateSut().Handle(
                new GetAllProformaInvoiceQuery { PageNumber = 2, PageSize = 5, SearchTerm = "x" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(23);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((new List<ProformaInvoiceDto>(), 0));

            await CreateSut().Handle(new GetAllProformaInvoiceQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
