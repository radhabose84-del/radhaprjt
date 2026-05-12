using AutoMapper;
using FAM.Application.Common.Interfaces.ISpecificationMaster;
using FAM.Application.SpecificationMaster.Queries.GetSpecificationMaster;
using FAM.Application.SpecificationMaster.Queries.GetSpecificationMasterAutoComplete;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.SpecificationMasters.Queries
{
    public sealed class GetSpecificationMasterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<ISpecificationMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetSpecificationMasterAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_WithResults_ReturnsMappedList()
        {
            var entities = new List<FAM.Domain.Entities.SpecificationMasters>
            {
                SpecificationMasterBuilders.ValidEntity()
            };
            var dtos = new List<SpecificationMasterAutoCompleteDTO>
            {
                SpecificationMasterBuilders.ValidAutoCompleteDto()
            };
            _mockQueryRepo
                .Setup(r => r.GetBySpecificationNameAsync(It.IsAny<int?>(), It.IsAny<string>()))
                .ReturnsAsync(entities);
            _mockMapper
                .Setup(m => m.Map<List<SpecificationMasterAutoCompleteDTO>>(entities))
                .Returns(dtos);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetSpecificationMasterAutoCompleteQuery { AssetGroupId = 1, SearchPattern = "Test" },
                CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_WithResults_PublishesAuditEvent()
        {
            var entities = new List<FAM.Domain.Entities.SpecificationMasters>
            {
                SpecificationMasterBuilders.ValidEntity()
            };
            var dtos = new List<SpecificationMasterAutoCompleteDTO>
            {
                SpecificationMasterBuilders.ValidAutoCompleteDto()
            };
            _mockQueryRepo
                .Setup(r => r.GetBySpecificationNameAsync(It.IsAny<int?>(), It.IsAny<string>()))
                .ReturnsAsync(entities);
            _mockMapper
                .Setup(m => m.Map<List<SpecificationMasterAutoCompleteDTO>>(entities))
                .Returns(dtos);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetSpecificationMasterAutoCompleteQuery { AssetGroupId = 1, SearchPattern = "Test" },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockQueryRepo
                .Setup(r => r.GetBySpecificationNameAsync(It.IsAny<int?>(), It.IsAny<string>()))
                .ReturnsAsync(new List<FAM.Domain.Entities.SpecificationMasters>());
            _mockMapper
                .Setup(m => m.Map<List<SpecificationMasterAutoCompleteDTO>>(It.IsAny<List<FAM.Domain.Entities.SpecificationMasters>>()))
                .Returns(new List<SpecificationMasterAutoCompleteDTO>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetSpecificationMasterAutoCompleteQuery { AssetGroupId = 1, SearchPattern = "none" },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_NullResult_ReturnsEmptyList()
        {
            _mockQueryRepo
                .Setup(r => r.GetBySpecificationNameAsync(It.IsAny<int?>(), It.IsAny<string>()))
                .ReturnsAsync((List<FAM.Domain.Entities.SpecificationMasters>)null!);
            _mockMapper
                .Setup(m => m.Map<List<SpecificationMasterAutoCompleteDTO>>(It.IsAny<List<FAM.Domain.Entities.SpecificationMasters>>()))
                .Returns(new List<SpecificationMasterAutoCompleteDTO>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetSpecificationMasterAutoCompleteQuery { AssetGroupId = 1, SearchPattern = "none" },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_NullSearchPattern_PassesEmptyStringToRepository()
        {
            string? capturedPattern = null;
            _mockQueryRepo
                .Setup(r => r.GetBySpecificationNameAsync(It.IsAny<int?>(), It.IsAny<string>()))
                .Callback<int?, string>((_, p) => capturedPattern = p)
                .ReturnsAsync(new List<FAM.Domain.Entities.SpecificationMasters>());
            _mockMapper
                .Setup(m => m.Map<List<SpecificationMasterAutoCompleteDTO>>(It.IsAny<List<FAM.Domain.Entities.SpecificationMasters>>()))
                .Returns(new List<SpecificationMasterAutoCompleteDTO>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetSpecificationMasterAutoCompleteQuery { AssetGroupId = 1, SearchPattern = null },
                CancellationToken.None);

            capturedPattern.Should().Be(string.Empty);
        }
    }
}
