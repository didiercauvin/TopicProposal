using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeReview.TopicManagement.Api.TopicProposal
{
    public class FakeCommandBus : ICommandBus
    {
        private readonly IServiceProvider _serviceProvider;

        public FakeCommandBus(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task Send<TCommand>(TCommand command, CancellationToken ct) where TCommand : class
        {
            using var scope = _serviceProvider.CreateScope();
            var commandHandler = scope.ServiceProvider.GetRequiredService<ICommandHandler<TCommand>>();

            await commandHandler.Handle(command, ct);
        }
    }

    public class TopicProposalSaga : IEventHandler<TopicRequested>
    {
        private readonly ICommandBus _commandBus;

        public TopicProposalSaga(ICommandBus commandBus)
        {
            _commandBus = commandBus;
        }

        public async Task Handle(TopicRequested @event, CancellationToken ct)
        {
            await _commandBus.Send(new ScheduleTopic(@event.Id), ct);
        }

        public async Task On(TopicRescheduleAccepted accepted, CancellationToken ct)
        {
            await _commandBus.Send(new ScheduleTopic(accepted.Id), ct);
        }
    }
}
