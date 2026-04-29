using SalesManagement.Domain.Entities;
using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesEnquiry;
using SalesManagement.Application.SalesEnquiry.Commands.CreateSalesEnquiry;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.SalesEnquiry.Commands;

public sealed class CreateSalesEnquiryCommandHandlerTests
{
    private readonly Mock<ISalesEnquiryCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<ISalesEnquiryQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
    private readonly Mock<IDocumentSequenceLookup> _mockDocSeqLookup = new(MockBehavior.Loose);
    private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);

    private CreateSalesEnquiryCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object,
            _mockDocSeqLookup.Object, _mockIpService.Object);

    private CreateSalesEnquiryCommand BuildCommand() =>
        new()
        {
            SalesEnquiryDetails = new CreateSalesEnquiryDto
            {
                PartyId = 1,
                EnquiryDate = DateTimeOffset.UtcNow,
                EnquiryTypeId = 245,
                SalesEnquiryDetails = new List<CreateSalesEnquiryDto.CreateSalesEnquiryDetailDto>
                {
                    new() { ItemId = 1, Quantity = 10m }
                }
            }
        };

    private void SetupHappyPath(int newId = 1)
    {
        _mockMapper
            .Setup(m => m.Map<SalesEnquiryHeader>(It.IsAny<object>()))
            .Returns(new SalesEnquiryHeader { PartyId = 1 });

        _mockIpService.Setup(s => s.GetUnitId()).Returns(1);

        _mockDocSeqLookup
            .Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(5);

        _mockDocSeqLookup
            .Setup(d => d.GenerateDocumentNumber(It.IsAny<int>()))
            .ReturnsAsync(new List<string> { "SE-0001" });

        _mockCommandRepo
            .Setup(r => r.CreateAsync(It.IsAny<SalesEnquiryHeader>(), It.IsAny<int>()))
            .ReturnsAsync(newId);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsNewId()
    {
        SetupHappyPath(42);
        var result = await CreateSut().Handle(BuildCommand(), CancellationToken.None);
        result.Should().Be(42);
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsCreateOnce()
    {
        SetupHappyPath(1);
        await CreateSut().Handle(BuildCommand(), CancellationToken.None);

        _mockCommandRepo.Verify(
            r => r.CreateAsync(It.IsAny<SalesEnquiryHeader>(), It.IsAny<int>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_PublishesAuditEvent()
    {
        SetupHappyPath(1);
        await CreateSut().Handle(BuildCommand(), CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e =>
                    e.ActionDetail == "Create" &&
                    e.ActionCode == "SALESENQUIRY_CREATE"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_CreateReturnsZero_ThrowsExceptionRules()
    {
        SetupHappyPath(0);
        var sut = CreateSut();

        Func<Task> act = async () => await sut.Handle(BuildCommand(), CancellationToken.None);

        await act.Should().ThrowAsync<ExceptionRules>()
            .WithMessage("*Creation Failed*");
    }
}
