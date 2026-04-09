using AutoMapper;
using FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetCategoryByCustodian;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetTransferIssue;
using FAM.Domain.Events;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetTransferIssue.Queries
{
    public sealed class GetCategoryByCustodianQueryHandlerTests
    {
        private readonly Mock<IAssetTransferQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetCategoryByCustodianQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidRequest_ReturnsCategoryList()
        {
            var entities = new List<GetCategoryByCustodianDto> { new() };
            var dtos = new List<GetCategoryByCustodianDto> { new() };

            _mockRepo
                .Setup(r => r.GetCategoryByCustodianAsync("CUST1", 1))
                .ReturnsAsync(entities);
            _mockMapper
                .Setup(m => m.Map<List<GetCategoryByCustodianDto>>(It.IsAny<object>()))
                .Returns(dtos);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetCategoryByCustodianQuery { CustodianId = "CUST1", DepartmentId = 1 },
                CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockRepo
                .Setup(r => r.GetCategoryByCustodianAsync("CUST1", 1))
                .ReturnsAsync(new List<GetCategoryByCustodianDto>());
            _mockMapper
                .Setup(m => m.Map<List<GetCategoryByCustodianDto>>(It.IsAny<object>()))
                .Returns(new List<GetCategoryByCustodianDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetCategoryByCustodianQuery { CustodianId = "CUST1", DepartmentId = 1 },
                CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
