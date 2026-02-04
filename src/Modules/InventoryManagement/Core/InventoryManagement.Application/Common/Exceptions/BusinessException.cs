namespace InventoryManagement.Application.Common.Exceptions
{
    public class BusinessException : Exception
    {
        /// <summary>Optional machine-readable code (e.g., "PUTAWAY_NO_CANDIDATE").</summary>
        public string? Code { get; }

        /// <summary>Optional single-field context (e.g., "Scope", "Quantity").</summary>
        public string? Field { get; }

        /// <summary>Optional multi-field errors (field -> messages).</summary>
        public IReadOnlyDictionary<string, string[]>? Errors { get; }

        // Message only
        public BusinessException(string message) : base(message) { }

        // Message + inner
        public BusinessException(string message, Exception inner) : base(message, inner) { }

        // Code + message (no field)
        public BusinessException(string code, string message) : base(message)
        {
            Code = code;
        }

        // Field + message (+ optional code)
        public BusinessException(string field, string message, string? code = null) : base(message)
        {
            Field = field;
            Code = code;
        }

        // Message + error bag (+ optional code)
        public BusinessException(string message, IDictionary<string, string[]> errors, string? code = null) : base(message)
        {
            Code = code;
            Errors = new Dictionary<string, string[]>(errors);
        }
    }
}
