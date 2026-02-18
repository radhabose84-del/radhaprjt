    namespace Contracts.Dtos.Users
    {
        public sealed class CurrencyDto
        {
            public int Id { get; set; }
            public string Code { get; set; } = default!;
            public string Name { get; set; } = default!;
        }
    }
