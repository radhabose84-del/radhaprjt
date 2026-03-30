using AutoMapper;
using InventoryManagement.Application.Common.Interfaces.IMiscMaster;
using InventoryManagement.Application.MiscMaster.Queries.GetMiscMaster;
using InventoryManagement.Application.MiscMaster.Queries.GetMiscMasterAutoComplete;
using InventoryManagement.UnitTests.TestData;
using MediatR;

namespace InventoryManagement.UnitTests.Application.MiscMaster.Queries
{
    public sealed class GetMiscMasterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMiscMasterAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsMatchingItems()
        {
            var entities = new List<InventoryManagement.Domain.Entities.MiscMaster>
            {
                MiscMasterBuilders.ValidEntity(1)
            };
            var dtos = new List<GetMiscMasterAutoCompleteDto>
            {
                new GetMiscMasterAutoCompleteDto { Id = 1, Code = "MISC001", Description = "Test" }
            };

            _mockQueryRepo
                .Setup(r => r.GetMiscMaster("test", null, null))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<GetMiscMasterAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetMiscMasterAutoCompleteQuery { SearchPattern = "test" }, CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockQueryRepo
                .Setup(r => r.GetMiscMaster(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>()))
                .ReturnsAsync(new List<InventoryManagement.Domain.Entities.MiscMaster>());

            _mockMapper
                .Setup(m => m.Map<List<GetMiscMasterAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(new List<GetMiscMasterAutoCompleteDto>());

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetMiscMasterAutoCompleteQuery { SearchPattern = "xyz" }, CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            _mockQueryRepo
                .Setup(r => r.GetMiscMaster(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>()))
                .ReturnsAsync(new List<InventoryManagement.Domain.Entities.MiscMaster>());

            _mockMapper
                .Setup(m => m.Map<List<GetMiscMasterAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(new List<GetMiscMasterAutoCompleteDto>());

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetMiscMasterAutoCompleteQuery { SearchPattern = "test" }, CancellationToken.None);

            _mockQueryRepo.Verify(
                r => r.GetMiscMaster(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>()),
                Times.Once);
        }
    }
}
