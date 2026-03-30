using MediatR;
using Microsoft.AspNetCore.Mvc;
using PartyManagement.Application.BankMaster;
using PartyManagement.Application.BankMaster.Command.Create;
using PartyManagement.Application.BankMaster.Command.Delete;
using PartyManagement.Application.BankMaster.Command.Update;
using PartyManagement.Application.BankMaster.Queries.GetBankMasterById;
using PartyManagement.Application.BankMaster.Queries.GetBankMastersAutocomplete;
using PartyManagement.Application.BankMaster.Queries.GetBankMastersPaged;
using PartyManagement.Presentation.Controllers;
using PartyManagement.UnitTests.TestData;
using Xunit;

namespace PartyManagement.UnitTests.Controllers
{
    public sealed class BankMasterControllerTests
    {
        // BankMasterController takes ISender, not IMediator
        private readonly Mock<ISender> _mockSender = new(MockBehavior.Strict);

        private BankMasterController CreateSut() => new(_mockSender.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            var entities = (IReadOnlyList<BankMasterDto>)new List<BankMasterDto> { BankMasterBuilders.ValidDto() };

            _mockSender
                .Setup(m => m.Send(It.IsAny<GetBankMastersPagedQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((entities, 1));

            var result = await CreateSut().GetAllAsync(1, 10, null, CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsSenderOnce()
        {
            var entities = (IReadOnlyList<BankMasterDto>)new List<BankMasterDto>();

            _mockSender
                .Setup(m => m.Send(It.IsAny<GetBankMastersPagedQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((entities, 0));

            await CreateSut().GetAllAsync(1, 10, null, CancellationToken.None);

            _mockSender.Verify(
                m => m.Send(It.IsAny<GetBankMastersPagedQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_ExistingId_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetBankMasterByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(BankMasterBuilders.ValidDto());

            var result = await CreateSut().GetByIdAsync(1, CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsCreatedResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<CreateBankMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateAsync(BankMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(201);
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            var autocompleteItems = (IReadOnlyList<AutocompleteItemDto>)new List<AutocompleteItemDto>
            {
                new AutocompleteItemDto(1, "ICICI Bank", "BNK001")
            };

            _mockSender
                .Setup(m => m.Send(It.IsAny<GetBankMastersAutocompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(autocompleteItems);

            var result = await CreateSut().GetAllAutocompleteAsync("ICICI", CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
