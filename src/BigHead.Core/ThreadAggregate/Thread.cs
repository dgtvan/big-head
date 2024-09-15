using Ardalis.SharedKernel;

namespace BigHead.Core.ThreadAggregate;
internal class Thread : EntityBase, IAggregateRoot
{
    public string? Name { get; private set; }

    public string? Description { get; private set; }

    private readonly List<Message> _messages = [];
    public IEnumerable<Message> Messages => _messages.AsReadOnly();

    public void AddMessage(Message message)
    {
        _messages.Add(message);
    }
}
