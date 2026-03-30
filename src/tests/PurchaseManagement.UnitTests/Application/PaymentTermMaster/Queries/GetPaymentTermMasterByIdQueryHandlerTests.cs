using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPaymentTermMaster;
using PurchaseManagement.Application.PaymentTermMaster.Queries.GetAllPaymentTermMaster;
using PurchaseManagement.Application.PaymentTermMaster.Queries.GetPaymentTermMasterById;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.PaymentTermMaster.Queries
{
    public sealed class GetPaymentTermMasterByIdQueryHandlerTests
    {
        private readonly Mock<IPaymentTermMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetPaymentTermMasterByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            var dto = PaymentTermMasterBuilders.ValidDto(1);
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(dto);
            _mockMapper
                .Setup(m => m.Map<PaymentTermMasterDto>(It.IsAny<object>()))
                .Returns(dto);

            var result = await CreateSut().Handle(
                new GetPaymentTermMasterByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ValidId_CallsGetByIdOnce()
        {
            var dto = PaymentTermMasterBuilders.ValidDto(1);
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(dto);
            _mockMapper
                .Setup(m => m.Map<PaymentTermMasterDto>(It.IsAny<object>()))
                .Returns(dto);

            await CreateSut().Handle(
                new GetPaymentTermMasterByIdQuery { Id = 1 }, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditEvent()
        {
            var dto = PaymentTermMasterBuilders.ValidDto(1);
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(dto);
            _mockMapper
                .Setup(m => m.Map<PaymentTermMasterDto>(It.IsAny<object>()))
                .Returns(dto);

            await CreateSut().Handle(
                new GetPaymentTermMasterByIdQuery { Id = 1 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
