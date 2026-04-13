using AutoMapper;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceRequest;
using MaintenanceManagement.Application.MaintenanceRequest.Queries.GetExistingVendorDetails;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MaintenanceRequest.Queries.BatchD
{
    public sealed class GetExistingVendorDetailsQueryHandlerBatchDTests
    {
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMaintenanceRequestQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private GetExistingVendorDetailsQueryHandler CreateSut() =>
            new(_mockMapper.Object, _mockMediator.Object, _mockQueryRepo.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            _mockQueryRepo.Setup(r => r.GetVendorDetails(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.ExistingVendorDetails>());
            _mockMapper.Setup(m => m.Map<List<GetExistingVendorDetailsDto>>(It.IsAny<object>()))
                .Returns(new List<GetExistingVendorDetailsDto>());

            var result = await CreateSut().Handle(
                new GetExistingVendorDetailsQuery { OldUnitCode = "U1", VendorCode = "V1" },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_WithVendorData_ReturnsMappedList()
        {
            _mockQueryRepo.Setup(r => r.GetVendorDetails(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.ExistingVendorDetails> { new() });
            _mockMapper.Setup(m => m.Map<List<GetExistingVendorDetailsDto>>(It.IsAny<object>()))
                .Returns(new List<GetExistingVendorDetailsDto> { new() { VendorCode = "V1" } });

            var result = await CreateSut().Handle(
                new GetExistingVendorDetailsQuery { OldUnitCode = "U1", VendorCode = "V1" },
                CancellationToken.None);

            result.Data.Count.Should().Be(1);
        }
    }
}
