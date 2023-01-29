using CloudEvents.Shared;
using CloudNative.CloudEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudEvents.Consumer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (var broker = new Broker("activemq:tcp://localhost:61616"))
            using (var subscription = broker.Subscribe<GitRepositorySizeChangeInfo>(CloudEventSources.GitService, CloudEventTypes.GitRepositorySizeChanged, Handler))
            {
                Console.WriteLine("Consumer is listening... Press enter to stop the consumer.");
                Console.ReadLine();
            };
        }

        static async Task Handler(CloudEvent ce, GitRepositorySizeChangeInfo data)
        {
            Console.WriteLine(
                    "Received repository size change notification. ProjectId = {0}, repository size = {1}, size status = {2}",
                    data.ProjectId, data.RepositorySizeInMb, data.SizeStatus);
        }
    }
}
