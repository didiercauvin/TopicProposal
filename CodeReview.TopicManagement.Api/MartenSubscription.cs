using Marten.Events.Projections;
using Marten.Events;
using Marten;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeReview.TopicManagement.Api.TopicProposal.GetRequestedTopics;
using Weasel.Core;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using CodeReview.TopicManagement.Api.TopicProposal;
using Marten.Storage;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Collections.Concurrent;

namespace CodeReview.TopicManagement.Api
{
    public static class StoreOptionsServices
    {
        public static StoreOptions SetStoreOption(IServiceProvider sp, IConfiguration config)
        {
            var options = new StoreOptions();
            var schemaName = Environment.GetEnvironmentVariable("SchemaName");
            if (!string.IsNullOrEmpty(schemaName))
            {
                options.Events.DatabaseSchemaName = schemaName;
                options.DatabaseSchemaName = schemaName;
            }
            options.Connection(config.GetConnectionString("Incidents"));
            options.UseDefaultSerialization(EnumStorage.AsString, nonPublicMembersStorage: NonPublicMembersStorage.All);

            options.Projections.Add<RequestedTopicHistoryTransformation>();

            options.Projections.Add(
                new MartenSubscription(new DummyEventsPublisher(sp)),
                ProjectionLifecycle.Async,
                "MartenSubscription");

            return options;
        }
    }

    public interface IEventBus
    {
        Task Publish(object @event, CancellationToken ct);
    }

    public class FakeEventBus : IEventBus
    {
        private readonly IServiceProvider _serviceProvider;
        private static readonly ConcurrentDictionary<Type, MethodInfo> PublishMethods = new();

        public FakeEventBus(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        private async Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct)
        {
            using var scope = _serviceProvider.CreateScope();
            var eventHandlers = scope.ServiceProvider.GetServices<IEventHandler<TEvent>>();

            foreach (var eventHandler in eventHandlers)
            {
                await eventHandler.Handle(@event, ct);
            }
        }

        public Task Publish(object @event, CancellationToken ct)
        {
            return (Task)GetGenericPublishFor(@event)
            .Invoke(this, new object[] { @event, ct })!;

        }
        private static MethodInfo GetGenericPublishFor(object @event) =>
        PublishMethods.GetOrAdd(@event.GetType(), eventType =>
            typeof(FakeEventBus)
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .Single(m => m.Name == nameof(PublishAsync) && m.GetGenericArguments().Any())
                .MakeGenericMethod(eventType)
        );
    }

    public interface IMartenEventsConsumer
    {
        Task ConsumeAsync(IReadOnlyList<StreamAction> streamActions, CancellationToken ct);
    }

    public class MartenSubscription : IProjection
    {
        private readonly IMartenEventsConsumer consumer;

        public MartenSubscription(IMartenEventsConsumer consumer)
        {
            this.consumer = consumer;
        }

        public void Apply(
            IDocumentOperations operations,
            IReadOnlyList<StreamAction> streams
        )
        {
            throw new NotSupportedException("Subscription should be only run asynchronously");
        }

        public Task ApplyAsync(
            IDocumentOperations operations,
            IReadOnlyList<StreamAction> streams,
            CancellationToken ct
        )
        {
            return consumer.ConsumeAsync(streams, ct);
        }
    }

    public class DummyEventsPublisher : IMartenEventsConsumer
    {
        private readonly IServiceProvider _serviceProvider;

        public DummyEventsPublisher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task ConsumeAsync(IReadOnlyList<StreamAction> streamActions, CancellationToken ct)
        {
            foreach (var @event in streamActions.SelectMany(streamAction => streamAction.Events))
            {
                using var scope = _serviceProvider.CreateScope();
                var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();

                eventBus.Publish(@event.Data, ct);
            }
            
            return Task.CompletedTask;
        }
    }
}
