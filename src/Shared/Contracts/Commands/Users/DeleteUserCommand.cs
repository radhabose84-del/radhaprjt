namespace Contracts.Commands.Users
{
    public class DeleteUserCommand
    {
        public int UserId { get; set; }
        public string Reason { get; set; } = default!;
    }
}