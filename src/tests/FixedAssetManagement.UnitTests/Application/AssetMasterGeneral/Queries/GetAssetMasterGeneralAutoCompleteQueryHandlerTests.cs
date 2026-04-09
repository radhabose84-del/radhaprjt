using AutoMapper;
using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral;
using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneralAutoComplete;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetMasterGeneral;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetMasterGeneral.Queries
{
    public sealed class GetAssetMasterGeneralAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IAssetMasterGeneralQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAssetMasterGeneralAutoCompleteQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidSearchPattern_ReturnsList()
        {
            var entities = new List<AssetMasterGeneralDTO> { new() };
            var dtos = new List<AssetMasterGeneralAutoCompleteDTO> { new() };

            _mockRepo
                .Setup(r => r.GetByAssetNameAsync("test"))
                .ReturnsAsync(entities);
            _mockMapper
                .Setup(m => m.Map<List<AssetMasterGeneralAutoCompleteDTO>>(It.IsAny<object>()))
                .Returns(dtos);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetMasterGeneralAutoCompleteQuery { SearchPattern = "test" }, CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_NullResult_ThrowsValidationException()
        {
            _mockRepo
                .Setup(r => r.GetByAssetNameAsync(""))
                .ReturnsAsync((List<AssetMasterGeneralDTO>?)null!);

            Func<Task> act = async () => await CreateSut().Handle(
                new GetAssetMasterGeneralAutoCompleteQuery { SearchPattern = null }, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task Handle_EmptyResult_ThrowsValidationException()
        {
            _mockRepo
                .Setup(r => r.GetByAssetNameAsync(""))
                .ReturnsAsync(new List<AssetMasterGeneralDTO>());

            Func<Task> act = async () => await CreateSut().Handle(
                new GetAssetMasterGeneralAutoCompleteQuery { SearchPattern = null }, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
        }
    }
}
