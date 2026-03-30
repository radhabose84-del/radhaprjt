using AutoMapper;
using Contracts.Common;
using PurchaseManagement.Application.Common.Interfaces.IServiceMaster;
using PurchaseManagement.Application.ServiceMaster.Commands.CreateService;
using PurchaseManagement.Application.ServiceMaster.Queries.GetAllServices;
using PurchaseManagement.UnitTests.TestData;
using MediatR;

namespace PurchaseManagement.UnitTests.Application.ServiceMaster.Commands
{
    public sealed class CreateServiceCommandHandlerTests
    {
        private readonly Mock<IServiceQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IServiceCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private CreateServiceCommandHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupHappyPath(int id = 1)
        {
            var entity = ServiceMasterBuilders.ValidEntity(id);
            var dto = ServiceMasterBuilders.ValidDto(id);

            _mockMapper
                .Setup(m => m.Map<PurchaseManagement.Domain.Entities.ServiceMaster>(It.IsAny<CreateServiceCommand>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<PurchaseManagement.Domain.Entities.ServiceMaster>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);

            _mockQueryRepo
                .Setup(r => r.GetServiceMasterByIdAsync(id))
                .ReturnsAsync(dto);

            _mockMapper
                .Setup(m => m.Map<GetServiceMasterDto>(dto))
                .Returns(dto);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsDto()
        {
            SetupHappyPath();

            var result = await CreateSut().Handle(ServiceMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsDtoWithServiceCode()
        {
            SetupHappyPath();

            var result = await CreateSut().Handle(ServiceMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            result.ServiceCode.Should().Be("SRV001");
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath();

            await CreateSut().Handle(ServiceMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<PurchaseManagement.Domain.Entities.ServiceMaster>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();

            await CreateSut().Handle(ServiceMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ZeroId_ThrowsExceptionRules()
        {
            var entity = ServiceMasterBuilders.ValidEntity(0);

            _mockMapper
                .Setup(m => m.Map<PurchaseManagement.Domain.Entities.ServiceMaster>(It.IsAny<CreateServiceCommand>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<PurchaseManagement.Domain.Entities.ServiceMaster>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);

            _mockQueryRepo
                .Setup(r => r.GetServiceMasterByIdAsync(0))
                .Returns(Task.FromResult<GetServiceMasterDto>(null!));

            Func<Task> act = async () => await CreateSut().Handle(ServiceMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>();
        }
    }
}
