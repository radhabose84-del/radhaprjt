using AutoMapper;
using MediatR;
using LogisticsManagement.Application.Common.Interfaces.IFreightMaster;
using LogisticsManagement.Application.FreightMaster.Dto;
using LogisticsManagement.Application.FreightMaster.Queries.GetAllFreightMaster;
using LogisticsManagement.Domain.Events;
using LogisticsManagement.UnitTests.TestData;

namespace LogisticsManagement.UnitTests.Application.FreightMaster.Queries
{
    public sealed class GetAllFreightMasterQueryHandlerTests
    {
        private readonly Mock<IFreightMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAllFreightMasterQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var dtoList = new List<FreightMasterDto> { FreightMasterBuilders.ValidDto() };
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((dtoList, 1));
            _mockMapper
                .Setup(m => m.Map<List<FreightMasterDto>>(dtoList))
                .Returns(dtoList);

            var result = await CreateSut().Handle(
                new GetAllFreightMasterQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var dtoList = new List<FreightMasterDto> { FreightMasterBuilders.ValidDto() };
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(2, 5, "search"))
                .ReturnsAsync((dtoList, 11));
            _mockMapper
                .Setup(m => m.Map<List<FreightMasterDto>>(dtoList))
                .Returns(dtoList);

            var result = await CreateSut().Handle(
                new GetAllFreightMasterQuery { PageNumber = 2, PageSize = 5, SearchTerm = "search" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            var empty = new List<FreightMasterDto>();
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((empty, 0));
            _mockMapper
                .Setup(m => m.Map<List<FreightMasterDto>>(empty))
                .Returns(empty);

            var result = await CreateSut().Handle(
                new GetAllFreightMasterQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            var dtoList = new List<FreightMasterDto> { FreightMasterBuilders.ValidDto() };
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((dtoList, 1));
            _mockMapper
                .Setup(m => m.Map<List<FreightMasterDto>>(dtoList))
                .Returns(dtoList);

            await CreateSut().Handle(
                new GetAllFreightMasterQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
