using SalesManagement.Domain.Entities;
using AutoMapper;
using Contracts.Common;
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

    private CreateSalesEnquiryCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

    private CreateSalesEnquiryCommand BuildCommand() =>
        new()
        {
            SalesEnquiryDetails = new CreateSalesEnquiryDto
            {
                PartyId = 1,
                EnquiryDate = DateTimeOffset.UtcNow,
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

        _mockCommandRepo
            .Setup(r => r.CreateAsync(It.IsAny<SalesEnquiryHeader>()))
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
            r => r.CreateAsync(It.IsAny<SalesEnquiryHeader>()),
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
