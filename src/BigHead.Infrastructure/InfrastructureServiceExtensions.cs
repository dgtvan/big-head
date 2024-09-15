using Ardalis.SharedKernel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using BigHead.Core.Interfaces;
using BigHead.Infrastructure.Data;
using BigHead.Infrastructure.Data.Queries;
using BigHead.Infrastructure.Email;
using BigHead.UseCases.Contributors.List;
using BigHead.UseCases.Projects.ListIncompleteItems;
using BigHead.UseCases.Projects.ListShallow;

namespace BigHead.Infrastructure;

public static class InfrastructureServiceExtensions
{
  public static IServiceCollection AddInfrastructureServices(
    this IServiceCollection services,
    ILogger logger,
    bool isDevelopment)
  {
    if (isDevelopment)
    {
      RegisterDevelopmentOnlyDependencies(services);
    }
    else
    {
      RegisterProductionOnlyDependencies(services);
    }
    
    RegisterEF(services);
    
    logger.LogInformation("{Project} services registered", "Infrastructure");
    
    return services;
  }

  private static void RegisterDevelopmentOnlyDependencies(IServiceCollection services)
  {
    services.AddScoped<IEmailSender, SmtpEmailSender>();
    services.AddScoped<IListContributorsQueryService, FakeListContributorsQueryService>();
    services.AddScoped<IListIncompleteItemsQueryService, FakeListIncompleteItemsQueryService>();
    services.AddScoped<IListProjectsShallowQueryService, FakeListProjectsShallowQueryService>();
  }
  
  private static void RegisterProductionOnlyDependencies(IServiceCollection services)
  {
    services.AddScoped<IEmailSender, SmtpEmailSender>();
    services.AddScoped<IListContributorsQueryService, ListContributorsQueryService>();
    services.AddScoped<IListIncompleteItemsQueryService, ListIncompleteItemsQueryService>();
    services.AddScoped<IListProjectsShallowQueryService, ListProjectsShallowQueryService>();
  }

  private static void RegisterEF(IServiceCollection services)
  {
    services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
    services.AddScoped(typeof(IReadRepository<>), typeof(EfRepository<>));
  }
}
