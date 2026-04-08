using AutoMapper;
using FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetCategoryByDeptId;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetTransferIssue;
using FAM.Domain.Events;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetTransferIssue.Queries
{
    public sealed class GetCategoryByDeptIQueryHandlerTests
    {
        private readonly Mock<IAssetTransferQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetCategoryByDeptIQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidDepartmentId_ReturnsCategoryList()
        {
            var entities = new List<GetCategoryByDeptIdDto> { new() };
            var dtos = new List<GetCategoryByDeptIdDto> { new() };

            _mockRepo
                .Setup(r => r.GetCategoriesByDepartmentAsync(1))
                .ReturnsAsync(entities);
            _mockMapper
                .Setup(m => m.Map<List<GetCategoryByDeptIdDto>>(It.IsAny<object>()))
                .Returns(dtos);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetCategoryByDeptIQuery { DepartmentId = 1 }, CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockRepo
                .Setup(r => r.GetCategoriesByDepartmentAsync(1))
                .ReturnsAsync(new List<GetCategoryByDeptIdDto>());
            _mockMapper
                .Setup(m => m.Map<List<GetCategoryByDeptIdDto>>(It.IsAny<object>()))
                .Returns(new List<GetCategoryByDeptIdDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetCategoryByDeptIQuery { DepartmentId = 1 }, CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
