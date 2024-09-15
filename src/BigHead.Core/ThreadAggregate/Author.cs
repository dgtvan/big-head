using Ardalis.SharedKernel;

namespace BigHead.Core.ThreadAggregate;
internal class Author : EntityBase
{
    public string MailAddress { get; private set; }

    public AuthorKind Kind { get; private set; }

    public Author(string mailAddress, AuthorKind kind)
    {
        MailAddress = mailAddress;
        Kind = kind;
    }
}
