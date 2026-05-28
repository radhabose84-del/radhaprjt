using AutoMapper;
using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IVendorEvaluationCriteria;
using PurchaseManagement.Application.VendorEvaluationCriteria.Dto;
using PurchaseManagement.Application.VendorEvaluationCriteria.Queries.GetAllVendorEvaluationCriteria;
using PurchaseManagement.Domain.Events;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.VendorEvaluationCriteria.Queries
{
    public sealed class GetAllVendorEvaluationCriteriaQueryHandlerTests
    {
        private readonly Mock<IVendorEvaluationCriteriaQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAllVendorEvaluationCriteriaQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var dtoList = new List<VendorEvaluationCriteriaDto> { VendorEvaluationCriteriaBuilders.ValidDto() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((dtoList, 1));
            _mockMapper.Setup(m => m.Map<List<VendorEvaluationCriteriaDto>>(It.IsAny<object>())).Returns(dtoList);

            var result = await CreateSut().Handle(
                new GetAllVendorEvaluationCriteriaQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var dtoList = new List<VendorEvaluationCriteriaDto> { VendorEvaluationCriteriaBuilders.ValidDto() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(2, 5, "Quality")).ReturnsAsync((dtoList, 11));
            _mockMapper.Setup(m => m.Map<List<VendorEvaluationCriteriaDto>>(It.IsAny<object>())).Returns(dtoList);

            var result = await CreateSut().Handle(
                new GetAllVendorEvaluationCriteriaQuery { PageNumber = 2, PageSize = 5, SearchTerm = "Quality" }, CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((new List<VendorEvaluationCriteriaDto>(), 0));
            _mockMapper.Setup(m => m.Map<List<VendorEvaluationCriteriaDto>>(It.IsAny<object>())).Returns(new List<VendorEvaluationCriteriaDto>());

            var result = await CreateSut().Handle(
                new GetAllVendorEvaluationCriteriaQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            var dtoList = new List<VendorEvaluationCriteriaDto> { VendorEvaluationCriteriaBuilders.ValidDto() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((dtoList, 1));
            _mockMapper.Setup(m => m.Map<List<VendorEvaluationCriteriaDto>>(It.IsAny<object>())).Returns(dtoList);

            await CreateSut().Handle(new GetAllVendorEvaluationCriteriaQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
