using Ardalis.Result;
using Ardalis.SharedKernel;

namespace BigHead.UseCases.Contributors.Get;

public record GetContributorQuery(int ContributorId) : IQuery<Result<ContributorDTO>>;
