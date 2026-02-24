#nullable disable
using Contracts.Common;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.MiscMaster.Dto;
using SalesManagement.Application.MiscMaster.Queries.GetMiscMasterById;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.MiscMaster.Queries
{
    public sealed class GetMiscMasterByIdQueryHandlerTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private GetMiscMasterByIdQueryHandler CreateSut() => new(_mockQueryRepo.Object);

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_ExistingId_ReturnsSuccess()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(MiscMasterBuilders.ValidDto());

            var result = await CreateSut().Handle(
                new GetMiscMasterByIdQuery { Id = 1 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ExistingId_ReturnsCorrectData()
        {
            var dto = MiscMasterBuilders.ValidDto(id: 3, code: "CODE003");
            _mockQueryRepo.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(dto);

            var result = await CreateSut().Handle(
                new GetMiscMasterByIdQuery { Id = 3 },
                CancellationToken.None);

            result.Data.Id.Should().Be(3);
            result.Data.Code.Should().Be("CODE003");
        }

        [Fact]
        public async Task Handle_ExistingId_ReturnsCorrectMiscTypeInfo()
        {
            var dto = MiscMasterBuilders.ValidDto(miscTypeId: 2, miscTypeCode: "MISC002");
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);

            var result = await CreateSut().Handle(
                new GetMiscMasterByIdQuery { Id = 1 },
                CancellationToken.None);

            result.Data.MiscTypeId.Should().Be(2);
            result.Data.MiscTypeCode.Should().Be("MISC002");
        }

        // ── Not Found ─────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_NonExistentId_ThrowsEntityNotFoundException()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((MiscMasterDto)null);

            var sut = CreateSut();
            Func<Task> act = async () => await sut.Handle(
                new GetMiscMasterByIdQuery { Id = 999 },
                CancellationToken.None);

            await act.Should().ThrowAsync<EntityNotFoundException>();
        }

        [Fact]
        public async Task Handle_NonExistentId_ThrowsWithIdInMessage()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((MiscMasterDto)null);

            var sut = CreateSut();
            Func<Task> act = async () => await sut.Handle(
                new GetMiscMasterByIdQuery { Id = 999 },
                CancellationToken.None);

            await act.Should().ThrowAsync<EntityNotFoundException>()
                     .WithMessage("*999*");
        }
    }
}
