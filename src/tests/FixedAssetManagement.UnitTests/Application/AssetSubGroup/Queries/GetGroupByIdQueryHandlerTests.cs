using AutoMapper;
using FAM.Application.AssetSubGroup.Queries.GetAssetGroupById;
using FAM.Application.AssetSubGroup.Queries.GetAssetSubGroup;
using FAM.Application.Common.Interfaces.IAssetSubGroup;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetSubGroup.Queries
{
    public sealed class GetGroupByIdQueryHandlerTests
    {
        private readonly Mock<IAssetSubGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetGroupByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidGroupId_ReturnsList()
        {
            var entities = new List<FAM.Domain.Entities.AssetSubGroup?> { new() { Id = 1 } };
            var dtos = new List<AssetSubGroupDto> { new() { Id = 1 } };

            _mockQueryRepo
                .Setup(r => r.GetByGroupIdAsync(1))
                .ReturnsAsync(entities);
            _mockMapper
                .Setup(m => m.Map<List<AssetSubGroupDto>>(It.IsAny<object>()))
                .Returns(dtos);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetGroupByIdQuery { GroupId = 1 }, CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_NullResult_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.GetByGroupIdAsync(99))
                .ReturnsAsync((List<FAM.Domain.Entities.AssetSubGroup?>?)null);

            Func<Task> act = async () => await CreateSut().Handle(
                new GetGroupByIdQuery { GroupId = 99 }, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
        }
    }
}
