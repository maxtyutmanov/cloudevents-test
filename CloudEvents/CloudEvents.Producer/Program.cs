// See https://aka.ms/new-console-template for more information
using CloudEvents.Shared;

using var broker = new Broker("activemq:tcp://localhost:61616");
await broker.Publish(CloudEventSources.GitService, CloudEventTypes.GitRepositorySizeChanged, new GitRepositorySizeChangeInfo
{
    ProjectId = "project-1",
    RepositorySizeInMb = 1050,
    SizeStatus = GitRepositorySizeStatus.Warning
});

Console.WriteLine("Published the event");
Console.ReadLine();