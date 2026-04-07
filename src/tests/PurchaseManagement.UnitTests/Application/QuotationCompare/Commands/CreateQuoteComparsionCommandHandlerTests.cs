using AutoMapper;
using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IOutbox;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IQuotationCompare;
using PurchaseManagement.Application.Quotation.QuotationCompare.Commands.CreateQuoteComparsion;
using PurchaseManagement.Application.Quotation.QuotationCompare.Commands.CreateQuoteComparision;
using PurchaseManagement.Domain.Entities.Quotation.QuotationCompare;

namespace PurchaseManagement.UnitTests.Application.QuotationCompare.Commands
{
    public sealed class CreateQuoteComparsionCommandHandlerTests
    {
        private readonly Mock<IQuotationCompareCommandRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IOutboxEventPublisher> _mockOutbox = new(MockBehavior.Loose);

        private CreateQuoteComparsionCommandHandler CreateSut() =>
            new(_mockRepo.Object, _mockMediator.Object, _mockMapper.Object, _mockOutbox.Object);

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            var header = new QuotationComparisonHeader
            {
                Id = 1,
                RfqId = 1,
                RfqCode = "RFQ001",
                QuotationConfirmedDetails = new List<QuotationComparisonDetail>()
            };

            _mockMapper
                .Setup(m => m.Map<QuotationComparisonHeader>(It.IsAny<object>()))
                .Returns(header);

            _mockMapper
                .Setup(m => m.Map<CreateQuoteComparisonReverseDto>(It.IsAny<object>()))
                .Returns(new CreateQuoteComparisonReverseDto());

            _mockRepo
                .Setup(r => r.AddAsync(It.IsAny<QuotationComparisonHeader>()))
                .ReturnsAsync(1);

            _mockRepo
                .Setup(r => r.GetByIdQuoteComparisonWorkFlowAsync(It.IsAny<int>()))
                .ReturnsAsync(new QuoteComparisonWorkFlowDto());

            var command = new CreateQuoteComparsionCommand
            {
                CreateQuoteComparsion = new CreateQuoteComparsionDto()
            };

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var header = new QuotationComparisonHeader
            {
                Id = 1,
                RfqId = 1,
                RfqCode = "RFQ001",
                QuotationConfirmedDetails = new List<QuotationComparisonDetail>()
            };

            _mockMapper
                .Setup(m => m.Map<QuotationComparisonHeader>(It.IsAny<object>()))
                .Returns(header);

            _mockMapper
                .Setup(m => m.Map<CreateQuoteComparisonReverseDto>(It.IsAny<object>()))
                .Returns(new CreateQuoteComparisonReverseDto());

            _mockRepo
                .Setup(r => r.AddAsync(It.IsAny<QuotationComparisonHeader>()))
                .ReturnsAsync(1);

            _mockRepo
                .Setup(r => r.GetByIdQuoteComparisonWorkFlowAsync(It.IsAny<int>()))
                .ReturnsAsync(new QuoteComparisonWorkFlowDto());

            var command = new CreateQuoteComparsionCommand
            {
                CreateQuoteComparsion = new CreateQuoteComparsionDto()
            };

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_FailedCreate_ThrowsExceptionRules()
        {
            _mockMapper
                .Setup(m => m.Map<QuotationComparisonHeader>(It.IsAny<object>()))
                .Returns(new QuotationComparisonHeader
                {
                    QuotationConfirmedDetails = new List<QuotationComparisonDetail>()
                });

            _mockRepo
                .Setup(r => r.AddAsync(It.IsAny<QuotationComparisonHeader>()))
                .ReturnsAsync(0);

            var command = new CreateQuoteComparsionCommand
            {
                CreateQuoteComparsion = new CreateQuoteComparsionDto()
            };

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>();
        }
    }
}
