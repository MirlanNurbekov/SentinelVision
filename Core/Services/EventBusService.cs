namespace Core.Services;

using Core.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

public class EventBusService : IEventBus
{
    private readonly IModel channel;

    public EventBusService(string host)
    {
        var factory = new ConnectionFactory { HostName = host };
        var connection = factory.CreateConnection();
        channel = connection.CreateModel();
    }

    public void Publish<T>(T @event)
    {
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event));
        channel.BasicPublish("", typeof(T).Name, null, body);
    }

    public void Subscribe<T>(Action<T> handler)
    {
        channel.QueueDeclare(typeof(T).Name, false, false, false);
        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (s, e) =>
        {
            var data = JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(e.Body.ToArray()));
            handler(data);
        };
        channel.BasicConsume(typeof(T).Name, true, consumer);
    }
}