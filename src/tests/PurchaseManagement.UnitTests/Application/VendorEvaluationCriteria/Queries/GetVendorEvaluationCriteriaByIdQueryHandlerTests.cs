using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IVendorEvaluationCriteria;
using PurchaseManagement.Application.VendorEvaluationCriteria.Dto;
using PurchaseManagement.Application.VendorEvaluationCriteria.Queries.GetVendorEvaluationCriteriaById;
using PurchaseManagement.Domain.Events;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.VendorEvaluationCriteria.Queries
{
    public sealed class GetVendorEvaluationCriteriaByIdQueryHandlerTests
    {
        private readonly Mock<IVendorEvaluationCriteriaQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetVendorEvaluationCriteriaByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            var dto = VendorEvaluationCriteriaBuilders.ValidDto();
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);
            _mockMapper.Setup(m => m.Map<VendorEvaluationCriteriaDto>(It.IsAny<object>())).Returns(dto);

            var result = await CreateSut().Handle(new GetVendorEvaluationCriteriaByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NonExistentId_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((VendorEvaluationCriteriaDto?)null);

            var result = await CreateSut().Handle(new GetVendorEvaluationCriteriaByIdQuery { Id = 99 }, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_ExistingId_PublishesAuditEvent()
        {
            var dto = VendorEvaluationCriteriaBuilders.ValidDto();
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);
            _mockMapper.Setup(m => m.Map<VendorEvaluationCriteriaDto>(It.IsAny<object>())).Returns(dto);

            await CreateSut().Handle(new GetVendorEvaluationCriteriaByIdQuery { Id = 1 }, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NonExistentId_DoesNotPublishAuditEvent()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((VendorEvaluationCriteriaDto?)null);

            await CreateSut().Handle(new GetVendorEvaluationCriteriaByIdQuery { Id = 99 }, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
