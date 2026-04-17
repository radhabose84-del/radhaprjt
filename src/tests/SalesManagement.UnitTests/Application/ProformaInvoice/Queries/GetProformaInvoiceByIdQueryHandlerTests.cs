using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IProformaInvoice;
using SalesManagement.Application.ProformaInvoice.Dto;
using SalesManagement.Application.ProformaInvoice.Queries.GetProformaInvoiceById;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.ProformaInvoice.Queries
{
    public sealed class GetProformaInvoiceByIdQueryHandlerTests
    {
        private readonly Mock<IProformaInvoiceQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetProformaInvoiceByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(5))
                .ReturnsAsync(new ProformaInvoiceDto { Id = 5 });

            var result = await CreateSut().Handle(new GetProformaInvoiceByIdQuery { Id = 5 }, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(5);
        }

        [Fact]
        public async Task Handle_MissingId_ReturnsNull_AndDoesNotPublish()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((ProformaInvoiceDto?)null);

            var result = await CreateSut().Handle(new GetProformaInvoiceByIdQuery { Id = 99 }, CancellationToken.None);

            result.Should().BeNull();
            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ExistingId_PublishesAuditEvent()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(5))
                .ReturnsAsync(new ProformaInvoiceDto { Id = 5 });

            await CreateSut().Handle(new GetProformaInvoiceByIdQuery { Id = 5 }, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
