using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.MiscMaster.Dto;
using SalesManagement.Application.MiscMaster.Queries.GetMiscMasterById;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.MiscMaster.Queries
{
    public sealed class GetMiscMasterByIdQueryHandlerTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly Mock<IMediator> _mockMediator = new();

        private GetMiscMasterByIdQueryHandler CreateSut()
        {
            _mockMapper.Setup(m => m.Map<MiscMasterDto>(It.IsAny<object>()))
                .Returns<object>(o => (o as MiscMasterDto)!);
            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return new GetMiscMasterByIdQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
        }

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_ExistingId_ReturnsNotNull()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(MiscMasterBuilders.ValidDto());

            var result = await CreateSut().Handle(
                new GetMiscMasterByIdQuery { Id = 1 },
                CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_ExistingId_ReturnsCorrectData()
        {
            var dto = MiscMasterBuilders.ValidDto(id: 3, code: "CODE003");
            _mockQueryRepo.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(dto);

            var result = await CreateSut().Handle(
                new GetMiscMasterByIdQuery { Id = 3 },
                CancellationToken.None);

            result!.Id.Should().Be(3);
            result!.Code.Should().Be("CODE003");
        }

        [Fact]
        public async Task Handle_ExistingId_ReturnsCorrectMiscTypeInfo()
        {
            var dto = MiscMasterBuilders.ValidDto(miscTypeId: 2, miscTypeCode: "MISC002");
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);

            var result = await CreateSut().Handle(
                new GetMiscMasterByIdQuery { Id = 1 },
                CancellationToken.None);

            result!.MiscTypeId.Should().Be(2);
            result!.MiscTypeCode.Should().Be("MISC002");
        }

        // ── Not Found ─────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_NonExistentId_ReturnsNull()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((MiscMasterDto?)null);

            var result = await CreateSut().Handle(
                new GetMiscMasterByIdQuery { Id = 999 },
                CancellationToken.None);

            result.Should().BeNull();
        }
    }
}
