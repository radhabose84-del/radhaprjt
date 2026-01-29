
namespace FAM.Application.Common.Exceptions
{
   public class ExceptionRules : Exception
    {
        public ExceptionRules(string message) : base(message) { }
    }

    public class EntityAlreadyExistsException : ExceptionRules
    {
        public EntityAlreadyExistsException(string entity, string field, string value)
            : base($"{entity} already exists with {field} = '{value}'.") { }
    }

    public class EntityNotFoundException : ExceptionRules
    {
        public EntityNotFoundException(string entity, object key)
            : base($"{entity} with ID '{key}' was not found.") { }
    }
}