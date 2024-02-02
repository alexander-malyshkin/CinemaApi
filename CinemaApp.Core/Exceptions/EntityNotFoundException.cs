using System.ComponentModel.DataAnnotations;

namespace CinemaApp.Core.Exceptions
{
    public sealed class EntityNotFoundException : ValidationException
    {
        public string? EntityName { get; }

        public EntityNotFoundException(string? entityName, string msg) : base(msg)
        {
            EntityName = entityName;
        }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(EntityName)}: {EntityName}, {nameof(Message)}: {Message}";
        }
    }
}
