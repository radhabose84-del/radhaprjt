using AutoMapper;
using Contracts.Common;
using FAM.Application.AssetMaster.AssetWarranty.Queries.GetAssetWarranty;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetWarranty;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetWarranty.Queries
{
    public sealed class GetAssetWarrantyQueryHandlerTests
    {
        private readonly Mock<IAssetWarrantyQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAssetWarrantyQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var dtos = new List<AssetWarrantyDTO> { AssetWarrantyBuilders.ValidDto(1) };

            _mockQueryRepo
                .Setup(r => r.GetAllAssetWarrantyAsync(1, 10, null))
                .ReturnsAsync((dtos, 1));

            _mockMapper
                .Setup(m => m.Map<List<AssetWarrantyDTO>>(It.IsAny<object>()))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetWarrantyQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var dtos = new List<AssetWarrantyDTO> { AssetWarrantyBuilders.ValidDto(1) };

            _mockQueryRepo
                .Setup(r => r.GetAllAssetWarrantyAsync(2, 5, "search"))
                .ReturnsAsync((dtos, 11));

            _mockMapper
                .Setup(m => m.Map<List<AssetWarrantyDTO>>(It.IsAny<object>()))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetWarrantyQuery { PageNumber = 2, PageSize = 5, SearchTerm = "search" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllAssetWarrantyAsync(1, 10, null))
                .ReturnsAsync((new List<AssetWarrantyDTO>(), 0));

            _mockMapper
                .Setup(m => m.Map<List<AssetWarrantyDTO>>(It.IsAny<object>()))
                .Returns(new List<AssetWarrantyDTO>());

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetWarrantyQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
