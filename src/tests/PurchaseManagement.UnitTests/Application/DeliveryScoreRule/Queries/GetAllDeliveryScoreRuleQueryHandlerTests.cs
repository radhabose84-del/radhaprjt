using AutoMapper;
using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IDeliveryScoreRule;
using PurchaseManagement.Application.DeliveryScoreRule.Dto;
using PurchaseManagement.Application.DeliveryScoreRule.Queries.GetAllDeliveryScoreRule;
using PurchaseManagement.Domain.Events;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.DeliveryScoreRule.Queries
{
    public sealed class GetAllDeliveryScoreRuleQueryHandlerTests
    {
        private readonly Mock<IDeliveryScoreRuleQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAllDeliveryScoreRuleQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var dtoList = new List<DeliveryScoreRuleDto> { DeliveryScoreRuleBuilders.ValidDto() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((dtoList, 1));
            _mockMapper.Setup(m => m.Map<List<DeliveryScoreRuleDto>>(It.IsAny<object>())).Returns(dtoList);
            var result = await CreateSut().Handle(new GetAllDeliveryScoreRuleQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var dtoList = new List<DeliveryScoreRuleDto> { DeliveryScoreRuleBuilders.ValidDto() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(2, 5, "On-time")).ReturnsAsync((dtoList, 11));
            _mockMapper.Setup(m => m.Map<List<DeliveryScoreRuleDto>>(It.IsAny<object>())).Returns(dtoList);
            var result = await CreateSut().Handle(new GetAllDeliveryScoreRuleQuery { PageNumber = 2, PageSize = 5, SearchTerm = "On-time" }, CancellationToken.None);
            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((new List<DeliveryScoreRuleDto>(), 0));
            _mockMapper.Setup(m => m.Map<List<DeliveryScoreRuleDto>>(It.IsAny<object>())).Returns(new List<DeliveryScoreRuleDto>());
            var result = await CreateSut().Handle(new GetAllDeliveryScoreRuleQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
