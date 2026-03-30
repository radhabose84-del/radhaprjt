using AutoMapper;
using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneralById;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetMasterGeneral;
using FAM.Domain.Entities;
using FAM.Domain.Events;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetMasterGeneral.Queries
{
    public sealed class GetAssetMasterGeneralByIdQueryHandlerTests
    {
        private readonly Mock<IAssetMasterGeneralQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAssetMasterGeneralByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            var entity = new AssetMasterGenerals { Id = 1, AssetName = "Test Asset" };
            var dto = new AssetMasterDTO { AssetName = "Test Asset", AssetCode = "AST001" };

            // The repo uses dynamic tuple — set up with Returns to avoid type mismatch
            _mockQueryRepo
                .Setup(r => r.GetAssetMasterByIdAsync(It.IsAny<int>()))
                .Returns(Task.FromResult<(dynamic, dynamic, IEnumerable<dynamic>, IEnumerable<dynamic>, IEnumerable<dynamic>, IEnumerable<dynamic>, dynamic, IEnumerable<dynamic>, IEnumerable<dynamic>)>(
                    (entity, null!, Enumerable.Empty<dynamic>(), Enumerable.Empty<dynamic>(),
                     Enumerable.Empty<dynamic>(), Enumerable.Empty<dynamic>(), null!, Enumerable.Empty<dynamic>(), Enumerable.Empty<dynamic>())));

            _mockMapper
                .Setup(m => m.Map<AssetMasterDTO>(It.IsAny<object>()))
                .Returns(dto);

            var result = await CreateSut().Handle(new GetAssetMasterGeneralByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result.AssetName.Should().Be("Test Asset");
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditEvent()
        {
            var entity = new AssetMasterGenerals { Id = 1, AssetName = "Test Asset" };
            var dto = new AssetMasterDTO { AssetName = "Test Asset" };

            _mockQueryRepo
                .Setup(r => r.GetAssetMasterByIdAsync(It.IsAny<int>()))
                .Returns(Task.FromResult<(dynamic, dynamic, IEnumerable<dynamic>, IEnumerable<dynamic>, IEnumerable<dynamic>, IEnumerable<dynamic>, dynamic, IEnumerable<dynamic>, IEnumerable<dynamic>)>(
                    (entity, null!, Enumerable.Empty<dynamic>(), Enumerable.Empty<dynamic>(),
                     Enumerable.Empty<dynamic>(), Enumerable.Empty<dynamic>(), null!, Enumerable.Empty<dynamic>(), Enumerable.Empty<dynamic>())));

            _mockMapper
                .Setup(m => m.Map<AssetMasterDTO>(It.IsAny<object>()))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(new GetAssetMasterGeneralByIdQuery { Id = 1 }, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
