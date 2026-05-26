using AutoMapper;
using Contracts.Interfaces;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IQuotationEntry;
using PurchaseManagement.Application.Quotations.QuotationEntry.DTOs;
using PurchaseManagement.Application.Quotations.QuotationEntry.Queries.GetAllQuotations;

namespace PurchaseManagement.UnitTests.Application.QuotationEntry.Queries
{
    public sealed class GetAllQuotationsQueryHandlerTests
    {
        private readonly Mock<IQuotationQueryRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);

        private GetAllQuotationsQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockIp.Object);

        [Fact]
        public async Task Handle_ReturnsItems()
        {
            var items = new List<QuotationListItemDto>();
            _mockRepo
                .Setup(r => r.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>()))
                .Returns(Task.FromResult<(List<QuotationListItemDto> Items, int Total)>((items, 0)));

            _mockMapper
                .Setup(m => m.Map<List<QuotationListItemDto>>(It.IsAny<object>()))
                .Returns(items);

            var (result, total) = await CreateSut().Handle(
                new GetAllQuotationsQuery { PageNumber = 1, PageSize = 20 },
                CancellationToken.None);

            result.Should().NotBeNull();
            total.Should().Be(0);
        }

        [Fact]
        public async Task Handle_WhenNoPartyId_CallsGetAllAsync()
        {
            _mockIp.Setup(i => i.GetPartyId()).Returns((int?)null);
            _mockRepo
                .Setup(r => r.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>()))
                .Returns(Task.FromResult<(List<QuotationListItemDto> Items, int Total)>(
                    (new List<QuotationListItemDto>(), 0)));

            await CreateSut().Handle(
                new GetAllQuotationsQuery { PageNumber = 1, PageSize = 20 },
                CancellationToken.None);

            _mockRepo.Verify(
                r => r.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>()),
                Times.Once);
            _mockRepo.Verify(
                r => r.GetMyQuotationsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_WhenPartyIdPresent_CallsGetMyQuotationsAsync()
        {
            _mockIp.Setup(i => i.GetPartyId()).Returns(42);
            _mockRepo
                .Setup(r => r.GetMyQuotationsAsync(42, 1, 20, It.IsAny<string?>()))
                .Returns(Task.FromResult<(List<QuotationListItemDto> Items, int Total)>(
                    (new List<QuotationListItemDto>(), 0)));

            await CreateSut().Handle(
                new GetAllQuotationsQuery { PageNumber = 1, PageSize = 20 },
                CancellationToken.None);

            _mockRepo.Verify(
                r => r.GetMyQuotationsAsync(42, 1, 20, It.IsAny<string?>()),
                Times.Once);
            _mockRepo.Verify(
                r => r.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_WhenPartyIdZero_CallsGetAllAsync()
        {
            _mockIp.Setup(i => i.GetPartyId()).Returns(0);
            _mockRepo
                .Setup(r => r.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>()))
                .Returns(Task.FromResult<(List<QuotationListItemDto> Items, int Total)>(
                    (new List<QuotationListItemDto>(), 0)));

            await CreateSut().Handle(
                new GetAllQuotationsQuery { PageNumber = 1, PageSize = 20 },
                CancellationToken.None);

            _mockRepo.Verify(
                r => r.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>()),
                Times.Once);
            _mockRepo.Verify(
                r => r.GetMyQuotationsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            _mockRepo
                .Setup(r => r.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>()))
                .Returns(Task.FromResult<(List<QuotationListItemDto> Items, int Total)>((new List<QuotationListItemDto>(), 0)));

            _mockMapper
                .Setup(m => m.Map<List<QuotationListItemDto>>(It.IsAny<object>()))
                .Returns(new List<QuotationListItemDto>());

            await CreateSut().Handle(
                new GetAllQuotationsQuery { PageNumber = 1, PageSize = 20 },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
