using Apache.NMS;
using Apache.NMS.ActiveMQ;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.NewtonsoftJson;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CloudEvents.Shared
{
    public class Broker : IDisposable
    {
        private static readonly CloudEventFormatter _formatter = new JsonEventFormatter();

        private readonly IConnection _conn;
        private readonly ISession _session;

        public Broker(string activeMqUrl)
        {
            var connFactory = new ConnectionFactory(activeMqUrl);
            _conn = connFactory.CreateConnection();
            _conn.Start();
            _session = _conn.CreateSession();
        }

        public void Dispose()
        {
            _session.Dispose();
            _conn.Dispose();
        }

        public async Task Publish<T>(Uri cloudEventSource, string cloudEventType, T data)
        {
            // TODO: cache producers?

            using var topic = await _session.GetTopicAsync(TopicNameMap.GetTopicName(cloudEventSource, cloudEventType));
            using var producer = _session.CreateProducer(topic);

            var ce = new CloudEvent
            {
                Source = cloudEventSource,
                Type = cloudEventType,
                Data = data,
                Id = Guid.NewGuid().ToString(),
                Time = DateTimeOffset.UtcNow
            };

            var payload = _formatter.EncodeStructuredModeMessage(ce, out _);
            // TODO: this probably can be optimized, no need to copy memory
            var msg = await producer.CreateBytesMessageAsync(payload.Span.ToArray());
            await producer.SendAsync(msg);
        }

        public async Task<IDisposable> Subscribe<T>(Uri cloudEventSource, string cloudEventType, Func<CloudEvent, T, Task> handler)
        {
            using var topic = await _session.GetTopicAsync(TopicNameMap.GetTopicName(cloudEventSource, cloudEventType));
            var consumer = _session.CreateConsumer(topic);
            return new Subscription<T>(consumer, handler);
        }

        private class Subscription<T> : IDisposable
        {
            private readonly IMessageConsumer _consumer;
            private readonly Func<CloudEvent, T, Task> _handler;

            public Subscription(IMessageConsumer consumer, Func<CloudEvent, T, Task> handler)
            {
                _consumer = consumer;
                _handler = handler;

                // it's actually fire & forget, but we don't care
                _consumer.Listener += async msg => await HandleInternal(msg);
            }

            public void Dispose()
            {
                _consumer.Dispose();
            }

            private async Task HandleInternal(IMessage message)
            {
                var bytesMessage = message as IBytesMessage;
                var bytes = bytesMessage.Content;
                using var ms = new MemoryStream(bytes);
                var ce = await Broker._formatter.DecodeStructuredModeMessageAsync(ms, null, null);
                JObject dataAsJObject = (JObject)ce.Data;
                var structuredData = dataAsJObject.ToObject<T>();
                await _handler(ce, structuredData);
            }
        }
    }
}
