using AutoMapper;
using FAM.Application.AssetMaster.AssetLocation.Queries.GetSubLocationById;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetLocation;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetLocation.Queries
{
    public sealed class GetSubLocationByIdQueryHandlerTests
    {
        private readonly Mock<IAssetLocationQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetSubLocationByIdQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsList()
        {
            var entities = new List<FAM.Domain.Entities.SubLocation> { new() };
            var dtos = new List<GetAssetSubLocationDto> { new() };

            _mockRepo
                .Setup(r => r.GetSublocationByIdAsync(1))
                .ReturnsAsync(entities);
            _mockMapper
                .Setup(m => m.Map<List<GetAssetSubLocationDto>>(It.IsAny<object>()))
                .Returns(dtos);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetSubLocationByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_NullResult_ThrowsValidationException()
        {
            _mockRepo
                .Setup(r => r.GetSublocationByIdAsync(99))
                .ReturnsAsync((List<FAM.Domain.Entities.SubLocation>?)null);

            Func<Task> act = async () => await CreateSut().Handle(
                new GetSubLocationByIdQuery { Id = 99 }, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
        }
    }
}
