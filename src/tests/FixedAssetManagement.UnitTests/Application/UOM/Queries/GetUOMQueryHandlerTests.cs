using AutoMapper;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IUOM;
using FAM.Application.UOM.Queries.GetUOMs;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.UOM.Queries
{
    public sealed class GetUOMQueryHandlerTests
    {
        private readonly Mock<IUOMQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetUOMHandlerQuery CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var entities = new List<FAM.Domain.Entities.UOM> { FAMUOMBuilders.ValidEntity() };
            var dtos = new List<UOMDto> { FAMUOMBuilders.ValidDto() };

            _mockQueryRepo
                .Setup(r => r.GetAllUOMAsync(1, 10, null))
                .ReturnsAsync((entities, 1));

            _mockMapper
                .Setup(m => m.Map<List<UOMDto>>(It.IsAny<object>()))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetUOMQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var entities = new List<FAM.Domain.Entities.UOM> { FAMUOMBuilders.ValidEntity() };
            var dtos = new List<UOMDto> { FAMUOMBuilders.ValidDto() };

            _mockQueryRepo
                .Setup(r => r.GetAllUOMAsync(2, 5, "search"))
                .ReturnsAsync((entities, 11));

            _mockMapper
                .Setup(m => m.Map<List<UOMDto>>(It.IsAny<object>()))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetUOMQuery { PageNumber = 2, PageSize = 5, SearchTerm = "search" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllUOMAsync(1, 10, null))
                .ReturnsAsync((new List<FAM.Domain.Entities.UOM>(), 0));

            _mockMapper
                .Setup(m => m.Map<List<UOMDto>>(It.IsAny<object>()))
                .Returns(new List<UOMDto>());

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetUOMQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
