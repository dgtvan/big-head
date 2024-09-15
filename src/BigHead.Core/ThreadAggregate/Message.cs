using Ardalis.SharedKernel;

namespace BigHead.Core.ThreadAggregate;

internal class Message : EntityBase
{
    public string Content { get; private set; }

    public Author Author { get; private set; }

    public Message(string content, Author author)
    {
        Content = content;
        Author = author;
    }
}
