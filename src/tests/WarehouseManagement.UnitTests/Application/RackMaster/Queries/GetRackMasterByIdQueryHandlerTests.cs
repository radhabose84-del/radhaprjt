using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Contracts.Dtos.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Inventory;
using MediatR;
using WarehouseManagement.Application.Common.Interfaces.IRackMaster;
using WarehouseManagement.Application.RackMaster.Queries.GetAllRackMaster;
using WarehouseManagement.Application.RackMaster.Queries.GetRackMasterById;
using WarehouseManagement.UnitTests.TestData;

namespace WarehouseManagement.UnitTests.Application.RackMaster.Queries
{
    public sealed class GetRackMasterByIdQueryHandlerTests
    {
        private readonly Mock<IRackMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterLookup> _mockMiscLookup = new(MockBehavior.Loose);
        private readonly Mock<IUOMLookup> _mockUomLookup = new(MockBehavior.Loose);

        private GetRackMasterByIdQueryHandler CreateSut() =>
            new(_mockMapper.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMiscLookup.Object, _mockUomLookup.Object);

        private void SetupLookups()
        {
            _mockUomLookup.Setup(l => l.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<UOMLookupDto>());
            _mockMiscLookup.Setup(l => l.GetMiscMasterByIdAsync(It.IsAny<string>())).ReturnsAsync(new List<MiscMasterLookupDto>());
        }

        [Fact]
        public async Task Handle_Found_ReturnsDto()
        {
            SetupLookups();
            var dto = RackMasterBuilders.ValidDto();
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);
            _mockMapper.Setup(m => m.Map<RackMasterDto>(It.IsAny<object>())).Returns(dto);

            var result = await CreateSut().Handle(new GetRackMasterByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsValidationException()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((RackMasterDto?)null);

            Func<Task> act = async () => await CreateSut().Handle(new GetRackMasterByIdQuery { Id = 999 }, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>().WithMessage("*not found*");
        }
    }
}
