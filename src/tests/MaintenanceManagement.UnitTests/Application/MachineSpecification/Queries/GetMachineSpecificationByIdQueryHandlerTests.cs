using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IMachineSpecification;
using MaintenanceManagement.Application.MachineSpecification.Command;
using MaintenanceManagement.Application.MachineSpecification.Queries.GetMachineSpecificationById;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MachineSpecification.Queries
{
    public sealed class GetMachineSpecificationByIdQueryHandlerTests
    {
        private readonly Mock<IMachineSpecificationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMachineSpecificationByIdQueryHandler CreateSut() => new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsSuccess()
        {
            var dtos = new List<MachineSpecificationDto> { new() };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new List<MachineSpecificationDto> { new() });
            _mockMapper.Setup(m => m.Map<List<MachineSpecificationDto>>(It.IsAny<object>())).Returns(dtos);

            var result = await CreateSut().Handle(new GetMachineSpecificationByIdQuery { Id = 1 }, CancellationToken.None);
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }
    }
}
