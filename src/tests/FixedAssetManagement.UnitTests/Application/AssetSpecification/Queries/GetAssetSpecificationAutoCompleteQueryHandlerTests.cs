using AutoMapper;
using FAM.Application.AssetMaster.AssetSpecification.Queries.GetAssetSpecificationAutoComplete;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetSpecification;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetSpecification.Queries
{
    public sealed class GetAssetSpecificationAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IAssetSpecificationQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAssetSpecificationAutoCompleteQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidSearchPattern_ReturnsList()
        {
            var entities = new List<AssetSpecificationJsonDto> { new() };
            var dtos = new List<AssetSpecificationJsonDto> { new() };

            _mockRepo
                .Setup(r => r.GetByAssetSpecificationNameAsync("test"))
                .ReturnsAsync(entities);
            _mockMapper
                .Setup(m => m.Map<List<AssetSpecificationJsonDto>>(It.IsAny<object>()))
                .Returns(dtos);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetSpecificationAutoCompleteQuery { SearchPattern = "test" }, CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_NullResult_ThrowsValidationException()
        {
            _mockRepo
                .Setup(r => r.GetByAssetSpecificationNameAsync(""))
                .ReturnsAsync((List<AssetSpecificationJsonDto>?)null!);

            Func<Task> act = async () => await CreateSut().Handle(
                new GetAssetSpecificationAutoCompleteQuery { SearchPattern = null }, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task Handle_EmptyResult_ThrowsValidationException()
        {
            _mockRepo
                .Setup(r => r.GetByAssetSpecificationNameAsync(""))
                .ReturnsAsync(new List<AssetSpecificationJsonDto>());

            Func<Task> act = async () => await CreateSut().Handle(
                new GetAssetSpecificationAutoCompleteQuery { SearchPattern = null }, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
        }
    }
}
