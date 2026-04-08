using AutoMapper;
using FAM.Application.AssetMaster.AssetWarranty.Queries.GetAssetWarranty;
using FAM.Application.AssetMaster.AssetWarranty.Queries.GetAssetWarrantyAutoComplete;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetWarranty;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetWarranty.Queries
{
    public sealed class GetAssetWarrantyAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IAssetWarrantyQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAssetWarrantyAutoCompleteQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidSearchPattern_ReturnsList()
        {
            var entities = new List<AssetWarrantyDTO> { new() };
            var dtos = new List<AssetWarrantyAutoCompleteDTO> { new() };

            _mockRepo
                .Setup(r => r.GetByAssetWarrantyNameAsync("test"))
                .ReturnsAsync(entities);
            _mockMapper
                .Setup(m => m.Map<List<AssetWarrantyAutoCompleteDTO>>(It.IsAny<object>()))
                .Returns(dtos);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetWarrantyAutoCompleteQuery { SearchPattern = "test" }, CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_NullResult_ThrowsValidationException()
        {
            _mockRepo
                .Setup(r => r.GetByAssetWarrantyNameAsync(""))
                .ReturnsAsync((List<AssetWarrantyDTO>?)null!);

            Func<Task> act = async () => await CreateSut().Handle(
                new GetAssetWarrantyAutoCompleteQuery { SearchPattern = null }, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task Handle_EmptyResult_ThrowsValidationException()
        {
            _mockRepo
                .Setup(r => r.GetByAssetWarrantyNameAsync(""))
                .ReturnsAsync(new List<AssetWarrantyDTO>());

            Func<Task> act = async () => await CreateSut().Handle(
                new GetAssetWarrantyAutoCompleteQuery { SearchPattern = null }, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
        }
    }
}
