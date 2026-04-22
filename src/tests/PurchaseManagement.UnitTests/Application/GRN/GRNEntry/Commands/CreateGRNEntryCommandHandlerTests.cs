using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IGRN.IGRNEntry;
using PurchaseManagement.Application.GRN.GRNEntry.Commands;
using PurchaseManagement.Application.GRN.GRNEntry.Commands.CreateGRNEntry;
using PurchaseManagement.Domain.Entities.GRN.GRNEntry;

namespace PurchaseManagement.UnitTests.Application.GRN.GRNEntry.Commands
{
    public sealed class CreateGRNEntryCommandHandlerTests
    {
        private readonly Mock<IGRNEntryCommandRepository> _mockCmdRepo = new(MockBehavior.Loose);
        private readonly Mock<IGRNEntryQueryRepository> _mockQryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);

        private CreateGRNEntryCommandHandler CreateSut() =>
            new(_mockCmdRepo.Object, _mockMapper.Object, _mockMediator.Object,
                _mockQryRepo.Object, _mockIp.Object);

        private void SetupHappyPath(int newId = 1)
        {
            _mockMapper
                .Setup(m => m.Map<GrnHeader>(It.IsAny<object>()))
                .Returns(new GrnHeader { GrnNo = null, GrnDetails = new List<GrnDetail>() });

            _mockCmdRepo
                .Setup(r => r.GenerateNextCodeAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync("GRN-001");

            _mockIp.Setup(i => i.GetUserId()).Returns(1);
            _mockIp.Setup(i => i.GetUserName()).Returns("test");
            _mockIp.Setup(i => i.GetSystemIPAddress()).Returns("127.0.0.1");

            _mockCmdRepo
                .Setup(r => r.CreateAsync(It.IsAny<GrnHeader>()))
                .ReturnsAsync(newId);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_SuccessfulCreate_ReturnsNewId()
        {
            SetupHappyPath(42);

            var command = new CreateGRNEntryCommand
            {
                GrnEntryCreate = new CreateGRNEntryDto
                {
                    GRNDetailsDtos = new List<CreateGRNEntryDto.CreateGRNDetailsDto>()
                }
            };

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(42);
        }

        [Fact]
        public async Task Handle_ZeroResult_ThrowsExceptionRules()
        {
            SetupHappyPath(0);

            var command = new CreateGRNEntryCommand
            {
                GrnEntryCreate = new CreateGRNEntryDto
                {
                    GRNDetailsDtos = new List<CreateGRNEntryDto.CreateGRNDetailsDto>()
                }
            };

            Func<Task> act = () => CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>();
        }

        [Fact]
        public async Task Handle_SuccessfulCreate_PublishesAuditEvent()
        {
            SetupHappyPath(1);

            var command = new CreateGRNEntryCommand
            {
                GrnEntryCreate = new CreateGRNEntryDto
                {
                    GRNDetailsDtos = new List<CreateGRNEntryDto.CreateGRNDetailsDto>()
                }
            };

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
