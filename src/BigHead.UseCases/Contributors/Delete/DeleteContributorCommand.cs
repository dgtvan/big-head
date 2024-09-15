using Ardalis.Result;
using Ardalis.SharedKernel;

namespace BigHead.UseCases.Contributors.Delete;

public record DeleteContributorCommand(int ContributorId) : ICommand<Result>;
