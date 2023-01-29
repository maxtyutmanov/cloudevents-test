// See https://aka.ms/new-console-template for more information
using CloudEvents.Shared;

using var broker = new Broker("activemq:tcp://localhost:61616");
using var subscription = broker.Subscribe<GitRepositorySizeChangeInfo>(CloudEventSources.GitService, CloudEventTypes.GitRepositorySizeChanged, async (_, data) =>
{
    Console.WriteLine(
        "Received repository size change notification. ProjectId = {0}, repository size = {1}, size status = {2}",
        data.ProjectId, data.RepositorySizeInMb, data.SizeStatus);
});

await broker.Publish(CloudEventSources.GitService, CloudEventTypes.GitRepositorySizeChanged, new GitRepositorySizeChangeInfo
{
    ProjectId = "project-1",
    RepositorySizeInMb = 1050,
    SizeStatus = GitRepositorySizeStatus.Warning
});

Console.WriteLine("Published the event");
Console.ReadLine();