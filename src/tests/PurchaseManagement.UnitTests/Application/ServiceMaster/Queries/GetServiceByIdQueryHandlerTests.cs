using AutoMapper;
using Contracts.Dtos.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Inventory;
using PurchaseManagement.Application.Common.Interfaces.IServiceMaster;
using PurchaseManagement.Application.ServiceMaster.Queries.GetAllServices;
using PurchaseManagement.Application.ServiceMaster.Queries.GetServiceById;
using PurchaseManagement.UnitTests.TestData;
using MediatR;

namespace PurchaseManagement.UnitTests.Application.ServiceMaster.Queries
{
    public sealed class GetServiceByIdQueryHandlerTests
    {
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IServiceQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IUOMLookup> _mockUomLookup = new(MockBehavior.Loose);
        private readonly Mock<IHSNLookup> _mockHsnLookup = new(MockBehavior.Loose);

        private GetServiceByIdQueryHandler CreateSut() =>
            new(_mockMapper.Object, _mockMediator.Object, _mockQueryRepo.Object, _mockUomLookup.Object, _mockHsnLookup.Object);

        private void SetupLookups()
        {
            _mockUomLookup
                .Setup(u => u.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UOMLookupDto>
                {
                    new UOMLookupDto { Id = 1, Code = "NOS", UOMName = "Nos" }
                });

            _mockHsnLookup
                .Setup(h => h.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<HSNLookupDto>
                {
                    new HSNLookupDto { Id = 1, HSNCode = "SAC001", Description = "Test SAC" }
                });
        }

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            var dto = ServiceMasterBuilders.ValidDto(1);

            _mockQueryRepo
                .Setup(r => r.GetServiceMasterByIdAsync(1))
                .ReturnsAsync(dto);

            _mockMapper
                .Setup(m => m.Map<GetServiceMasterDto>(dto))
                .Returns(dto);

            SetupLookups();

            var result = await CreateSut().Handle(new GetServiceByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsKeyNotFoundException()
        {
            _mockQueryRepo
                .Setup(r => r.GetServiceMasterByIdAsync(99))
                .Returns(Task.FromResult<GetServiceMasterDto>(null!));

            Func<Task> act = async () => await CreateSut().Handle(new GetServiceByIdQuery { Id = 99 }, CancellationToken.None);

            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task Handle_ValidQuery_CallsGetByIdOnce()
        {
            var dto = ServiceMasterBuilders.ValidDto(1);

            _mockQueryRepo
                .Setup(r => r.GetServiceMasterByIdAsync(1))
                .ReturnsAsync(dto);

            _mockMapper
                .Setup(m => m.Map<GetServiceMasterDto>(dto))
                .Returns(dto);

            SetupLookups();

            await CreateSut().Handle(new GetServiceByIdQuery { Id = 1 }, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetServiceMasterByIdAsync(1), Times.Once);
        }
    }
}
