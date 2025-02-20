using System.Dynamic;

namespace Domain.Core.Authors
{
    public class Author
    {
        public int Id { get; init; }
        public required string ReferenceId { get; init; }
        public required string Name { get; init; }

        public static Author Create(int id, string referenceId, string name)
        {
            return new Author()
            {
                Id          = id,
                ReferenceId = referenceId,
                Name        = name
            };
        }
    }
}