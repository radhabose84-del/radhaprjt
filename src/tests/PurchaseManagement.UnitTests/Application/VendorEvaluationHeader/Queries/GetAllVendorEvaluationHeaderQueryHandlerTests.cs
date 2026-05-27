using AutoMapper;
using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IVendorEvaluationHeader;
using PurchaseManagement.Application.VendorEvaluationHeader.Dto;
using PurchaseManagement.Application.VendorEvaluationHeader.Queries.GetAllVendorEvaluationHeader;
using PurchaseManagement.Domain.Events;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.VendorEvaluationHeader.Queries
{
    public sealed class GetAllVendorEvaluationHeaderQueryHandlerTests
    {
        private readonly Mock<IVendorEvaluationHeaderQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAllVendorEvaluationHeaderQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var dtoList = new List<VendorEvaluationHeaderDto> { VendorEvaluationHeaderBuilders.ValidDto() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((dtoList, 1));
            _mockMapper.Setup(m => m.Map<List<VendorEvaluationHeaderDto>>(It.IsAny<object>())).Returns(dtoList);

            var result = await CreateSut().Handle(
                new GetAllVendorEvaluationHeaderQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var dtoList = new List<VendorEvaluationHeaderDto> { VendorEvaluationHeaderBuilders.ValidDto() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(2, 5, "EVL")).ReturnsAsync((dtoList, 11));
            _mockMapper.Setup(m => m.Map<List<VendorEvaluationHeaderDto>>(It.IsAny<object>())).Returns(dtoList);

            var result = await CreateSut().Handle(
                new GetAllVendorEvaluationHeaderQuery { PageNumber = 2, PageSize = 5, SearchTerm = "EVL" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((new List<VendorEvaluationHeaderDto>(), 0));
            _mockMapper.Setup(m => m.Map<List<VendorEvaluationHeaderDto>>(It.IsAny<object>())).Returns(new List<VendorEvaluationHeaderDto>());

            var result = await CreateSut().Handle(
                new GetAllVendorEvaluationHeaderQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            var dtoList = new List<VendorEvaluationHeaderDto> { VendorEvaluationHeaderBuilders.ValidDto() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((dtoList, 1));
            _mockMapper.Setup(m => m.Map<List<VendorEvaluationHeaderDto>>(It.IsAny<object>())).Returns(dtoList);

            await CreateSut().Handle(
                new GetAllVendorEvaluationHeaderQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
