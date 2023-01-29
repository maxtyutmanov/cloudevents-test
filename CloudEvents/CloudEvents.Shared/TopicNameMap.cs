using System;
using System.Collections.Generic;
using System.Text;

namespace CloudEvents.Shared
{
    internal static class TopicNameMap
    {
        private static readonly Dictionary<string, string> _map;

        static TopicNameMap()
        {
            _map = new Dictionary<string, string>()
            {
                [GetKey(CloudEventSources.GitService, CloudEventTypes.GitRepositorySizeChanged)] = "GitService"
            };
        }

        internal static string GetTopicName(Uri cloudEventSource, string cloudEventType)
        {
            if (_map.TryGetValue(GetKey(cloudEventSource, cloudEventType), out var topicName))
            {
                return topicName;
            }

            throw new Exception($"Topic for event source {cloudEventSource} and event type {cloudEventType} was not found");
        }

        private static string GetKey(Uri cloudEventSource, string cloudEventType)
        {
            return $"source:{cloudEventSource}|type:{cloudEventType}";
        }
    }
}
