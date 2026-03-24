using AutoMapper;
using BudgetManagement.Application.Common.Interfaces.IMiscTypeMaster;
using BudgetManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using BudgetManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterById;
using BudgetManagement.UnitTests.TestData;
using MediatR;

namespace BudgetManagement.UnitTests.Application.MiscTypeMaster.Queries
{
    public sealed class GetMiscTypeMasterByIdQueryHandlerTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMiscTypeMasterByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsSuccess()
        {
            var entity = MiscTypeMasterBuilders.ValidEntity();
            var dto = MiscTypeMasterBuilders.ValidDto();

            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(entity);
            _mockMapper.Setup(m => m.Map<GetMiscTypeMasterDto>(entity)).Returns(dto);

            var result = await CreateSut().Handle(
                new GetMiscTypeMasterByIdQuery { Id = 1 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_NonExistingId_ReturnsNotFound()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((BudgetManagement.Domain.Entities.MiscTypeMaster)null!);

            var result = await CreateSut().Handle(
                new GetMiscTypeMasterByIdQuery { Id = 999 }, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_ExistingId_ReturnsCorrectDto()
        {
            var entity = MiscTypeMasterBuilders.ValidEntity(42);
            var dto = MiscTypeMasterBuilders.ValidDto(42, "MTY042", "Found Type");

            _mockQueryRepo.Setup(r => r.GetByIdAsync(42)).ReturnsAsync(entity);
            _mockMapper.Setup(m => m.Map<GetMiscTypeMasterDto>(entity)).Returns(dto);

            var result = await CreateSut().Handle(
                new GetMiscTypeMasterByIdQuery { Id = 42 }, CancellationToken.None);

            result.Data!.Id.Should().Be(42);
            result.Data.MiscTypeCode.Should().Be("MTY042");
        }
    }
}
