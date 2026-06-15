using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IGRN.IGRNEntry;
using PurchaseManagement.Application.GRN.GRNEntry.Commands;
using PurchaseManagement.Application.GRN.GRNEntry.Commands.CreateGRNEntry;
using PurchaseManagement.Domain.Entities.GRN.GRNEntry;
using static PurchaseManagement.Application.GRN.GRNEntry.Commands.CreateGRNEntry.CreateGRNEntryDto;

namespace PurchaseManagement.UnitTests.Application.GRN.GRNEntry.Commands
{
    public sealed class CreateGRNEntryCommandHandlerTests
    {
        private readonly Mock<IGRNEntryCommandRepository> _mockCmdRepo = new(MockBehavior.Loose);
        private readonly Mock<IGRNEntryQueryRepository> _mockQryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IDocumentSequenceLookup> _mockDocSeq = new(MockBehavior.Loose);

        private CreateGRNEntryCommandHandler CreateSut() =>
            new(_mockCmdRepo.Object, _mockMapper.Object, _mockMediator.Object,
                _mockQryRepo.Object, _mockIp.Object, _mockDocSeq.Object);

        // GrnNo + DocNo are produced via Finance.DocumentSequence. Wire the lookup so the
        // handler resolves a TransactionTypeId and a generated GrnNo before persisting.
        private void SetupDocumentSequence()
        {
            _mockDocSeq
                .Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(5);
            _mockDocSeq
                .Setup(d => d.GenerateDocumentNumber(It.IsAny<int>()))
                .ReturnsAsync((IReadOnlyList<string>)new List<string> { "GRN-001" });
        }

        private void SetupHappyPath(int newId = 1)
        {
            _mockMapper
                .Setup(m => m.Map<GrnHeader>(It.IsAny<object>()))
                .Returns(new GrnHeader { GrnNo = null, GrnDetails = new List<GrnDetail>() });

            SetupDocumentSequence();

            _mockIp.Setup(i => i.GetUnitId()).Returns(1);
            _mockIp.Setup(i => i.GetUserId()).Returns(1);
            _mockIp.Setup(i => i.GetUserName()).Returns("test");
            _mockIp.Setup(i => i.GetSystemIPAddress()).Returns("127.0.0.1");

            _mockCmdRepo
                .Setup(r => r.CreateAsync(It.IsAny<GrnHeader>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
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
        public async Task Handle_LineItemWithImage_PassesGrnDetailImageToEntity()
        {
            GrnHeader captured = null!;

            _mockMapper
                .Setup(m => m.Map<GrnHeader>(It.IsAny<object>()))
                .Returns(new GrnHeader { GrnNo = null, GrnDetails = new List<GrnDetail>() });
            SetupDocumentSequence();
            _mockIp.Setup(i => i.GetUnitId()).Returns(1);
            // Non-TEMP filename → passed straight through, no file rename/IO
            _mockQryRepo
                .Setup(r => r.GetDocumentDirectoryAsync())
                .ReturnsAsync($"UT_{Guid.NewGuid():N}");
            _mockQryRepo
                .Setup(r => r.GetPoOtherDetails(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new List<PoValueDetailsDto> { new PoValueDetailsDto { UnitPrice = 10m } });
            _mockCmdRepo
                .Setup(r => r.CreateAsync(It.IsAny<GrnHeader>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Callback<GrnHeader, int, CancellationToken>((h, _, __) => captured = h)
                .ReturnsAsync(1);

            var command = new CreateGRNEntryCommand
            {
                GrnEntryCreate = new CreateGRNEntryDto
                {
                    GRNDetailsDtos = new List<CreateGRNDetailsDto>
                    {
                        new CreateGRNDetailsDto
                        {
                            PoId = 1, ItemId = 1, OrderQuantity = 10m, DcQuantity = 5m,
                            GrnDetailImage = "line1.png"
                        }
                    }
                }
            };

            await CreateSut().Handle(command, CancellationToken.None);

            captured.Should().NotBeNull();
            captured.GrnDetails.Should().ContainSingle();
            captured.GrnDetails!.First().GrnDetailImage.Should().Be("line1.png");
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
