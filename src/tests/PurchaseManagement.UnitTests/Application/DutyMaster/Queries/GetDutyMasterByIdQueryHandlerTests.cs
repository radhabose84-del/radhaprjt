using AutoMapper;
using Contracts.Dtos.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Inventory;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IPurchase.DutyMaster;
using PurchaseManagement.Application.DutyMaster;
using PurchaseManagement.Application.DutyMaster.Queries.GetDutyMasterById;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.DutyMaster.Queries
{
    public sealed class GetDutyMasterByIdQueryHandlerTests
    {
        private readonly Mock<IDutyMasterQueryRepository> _mockReadRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IHSNLookup> _mockHsnLookup = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterCommandRepository> _mockMiscRepo = new(MockBehavior.Loose);

        private GetDutyMasterByIdQueryHandler CreateSut() =>
            new(_mockReadRepo.Object, _mockMapper.Object, _mockMediator.Object,
                _mockHsnLookup.Object, _mockMiscRepo.Object);

        private void SetupHappyPath(int id = 1)
        {
            var entity = DutyMasterBuilders.ValidEntity(id);
            var viewDto = new DutyMasterViewDto
            {
                Id = id,
                TariffNumber = entity.TariffNumber,
                DutyCategoryId = entity.DutyCategoryId,
                CountryOfOriginApplicability = entity.CountryOfOriginApplicability
            };

            _mockReadRepo
                .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<DutyMasterViewDto>(It.IsAny<object>()))
                .Returns(viewDto);

            _mockMiscRepo
                .Setup(r => r.GetManyByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<int, PurchaseManagement.Domain.Entities.MiscMaster>());

            _mockHsnLookup
                .Setup(h => h.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<HSNLookupDto>());
        }

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            SetupHappyPath();

            var result = await CreateSut().Handle(new GetDutyMasterByIdQuery(1), CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NotFound_ReturnsNull()
        {
            _mockReadRepo
                .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PurchaseManagement.Domain.Entities.DutyMaster?)null);

            var result = await CreateSut().Handle(new GetDutyMasterByIdQuery(99), CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_ValidId_CallsGetByIdOnce()
        {
            SetupHappyPath();

            await CreateSut().Handle(new GetDutyMasterByIdQuery(1), CancellationToken.None);

            _mockReadRepo.Verify(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
