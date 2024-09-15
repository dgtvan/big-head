using System.Text;
using Newtonsoft.Json;
using Xunit;
using BigHead.Web.ProjectEndpoints;
using Ardalis.HttpClientTestExtensions;
using BigHead.Web.Endpoints.Projects;
using BigHead.Web.Projects;
using FluentAssertions;

namespace BigHead.FunctionalTests.Projects;

[Collection("Sequential")]
public class ProjectItemMarkComplete : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client;

  public ProjectItemMarkComplete(CustomWebApplicationFactory<Program> factory)
  {
    _client = factory.CreateClient();
  }

  [Fact]
  public async Task MarksIncompleteItemComplete()
  {
    var projectId = 1;
    var itemId = 1;

    var jsonContent = new StringContent(JsonConvert.SerializeObject(null), Encoding.UTF8, "application/json");

    var route = MarkItemCompleteRequest.BuildRoute(projectId, itemId);
    var response = await _client.PostAsync(route, jsonContent);
    response.EnsureSuccessStatusCode();

    var stringResponse = await response.Content.ReadAsStringAsync();
    Assert.Equal("", stringResponse);

    // confirm item is complete
    var project = await _client.GetAndDeserializeAsync<GetProjectByIdResponse>(GetProjectByIdRequest.BuildRoute(projectId));
    project.Items.First(i => i.Id == itemId).IsDone.Should().BeTrue();
  }
}
