using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IVendorEvaluationHeader;
using PurchaseManagement.Application.VendorEvaluationHeader.Dto;
using PurchaseManagement.Application.VendorEvaluationHeader.Queries.GetVendorEvaluationHeaderById;
using PurchaseManagement.Domain.Events;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.VendorEvaluationHeader.Queries
{
    public sealed class GetVendorEvaluationHeaderByIdQueryHandlerTests
    {
        private readonly Mock<IVendorEvaluationHeaderQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetVendorEvaluationHeaderByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            var dto = VendorEvaluationHeaderBuilders.ValidDto();
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);
            _mockMapper.Setup(m => m.Map<VendorEvaluationHeaderDto>(It.IsAny<object>())).Returns(dto);

            var result = await CreateSut().Handle(new GetVendorEvaluationHeaderByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ExistingId_IncludesDetails()
        {
            var dto = VendorEvaluationHeaderBuilders.ValidDto();
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);
            _mockMapper.Setup(m => m.Map<VendorEvaluationHeaderDto>(It.IsAny<object>())).Returns(dto);

            var result = await CreateSut().Handle(new GetVendorEvaluationHeaderByIdQuery { Id = 1 }, CancellationToken.None);

            result!.VendorEvaluationDetails.Should().NotBeNull();
            result.VendorEvaluationDetails!.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_NonExistentId_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((VendorEvaluationHeaderDto?)null);

            var result = await CreateSut().Handle(new GetVendorEvaluationHeaderByIdQuery { Id = 99 }, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_ExistingId_PublishesAuditEvent()
        {
            var dto = VendorEvaluationHeaderBuilders.ValidDto();
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);
            _mockMapper.Setup(m => m.Map<VendorEvaluationHeaderDto>(It.IsAny<object>())).Returns(dto);

            await CreateSut().Handle(new GetVendorEvaluationHeaderByIdQuery { Id = 1 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "GetById" &&
                        e.ActionCode == "GetVendorEvaluationHeaderByIdQuery"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NonExistentId_DoesNotPublishAuditEvent()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((VendorEvaluationHeaderDto?)null);

            await CreateSut().Handle(new GetVendorEvaluationHeaderByIdQuery { Id = 99 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
