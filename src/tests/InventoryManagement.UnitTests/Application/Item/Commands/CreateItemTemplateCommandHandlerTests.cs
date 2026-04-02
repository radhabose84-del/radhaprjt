using InventoryManagement.Application.Item.ItemDetail.Commands.CreateItemTemplate;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;

namespace InventoryManagement.UnitTests.Application.Item.Commands
{
    // CreateItemTemplateCommandHandler fields have no DI constructor (pragma CS0649 suppressed).
    // Only the HasVariants guard is testable via constructor-less instantiation.
    public sealed class CreateItemTemplateCommandHandlerTests
    {
        private static CreateItemTemplateCommandHandler CreateSut() => new();

        private static CreateItemTemplateCommand BuildCommand(bool hasVariants) =>
            new()
            {
                Payload = new ItemDto
                {
                    ItemName = "Widget Template",
                    HasVariants = hasVariants,
                    ItemGroupId = 1,
                    ItemCategoryId = 2,
                    VariantAttributes = new List<VariantAttributeDto>(),
                    VariantValues = new List<VariantValueDto>()
                }
            };

        [Fact]
        public async Task Handle_HasVariantsFalse_ThrowsInvalidOperationException()
        {
            var sut = CreateSut();

            Func<Task> act = () => sut.Handle(BuildCommand(hasVariants: false), CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*HasVariants=true*");
        }

        [Fact]
        public async Task Handle_HasVariantsTrue_DoesNotThrowGuardException()
        {
            var sut = CreateSut();

            // Fields are unassigned (CS0649), so NullReferenceException follows the guard —
            // but the guard itself must NOT fire when HasVariants = true.
            Func<Task> act = () => sut.Handle(BuildCommand(hasVariants: true), CancellationToken.None);

            var exception = await Record.ExceptionAsync(act);
            exception.Should().NotBeOfType<InvalidOperationException>(
                "the HasVariants guard should pass when HasVariants is true");
        }
    }
}
