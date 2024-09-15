using Ardalis.Result;
using Ardalis.SharedKernel;
using Ardalis.Specification;
using FluentAssertions;
using BigHead.Core.ContributorAggregate;
using BigHead.Core.ContributorAggregate.Specifications;
using BigHead.UseCases.Contributors.Get;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Xunit;

namespace BigHead.UnitTests.UseCases.Contributors;

public class GetContributorHandlerHandle
{
  private readonly string _testName = "test name";
  private readonly IReadRepository<Contributor> _repository = Substitute.For<IReadRepository<Contributor>>();
  private GetContributorHandler _handler;

  public GetContributorHandlerHandle()
  {
      _handler = new GetContributorHandler(_repository);
  }

  [Fact]
  public async Task ReturnsRecordGivenValidId()
  {
    int validId = 1;
    _repository.FirstOrDefaultAsync(Arg.Any<ContributorByIdSpec>(), Arg.Any<CancellationToken>())
      .Returns(new Contributor(_testName));
    var result = await _handler.Handle(new GetContributorQuery(validId), CancellationToken.None);

    result.IsSuccess.Should().BeTrue();
    result.Value.Name.Should().Be(_testName);
  }

  [Fact]
  public async Task ReturnsNotFoundGivenInvalidId()
  {
    int invalidId = 1000;
    _repository.FirstOrDefaultAsync(Arg.Any<ContributorByIdSpec>(), Arg.Any<CancellationToken>()).ReturnsNull();
    var result = await _handler.Handle(new GetContributorQuery(invalidId), CancellationToken.None);

    result.IsSuccess.Should().BeFalse();
    result.Status.Should().Be(ResultStatus.NotFound);
  }
}
