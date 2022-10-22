using CodeReview.TopicManagement.Api.TopicProposal;
using Marten;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeReview.TopicManagement.Api
{
    public class CommandHandler<TState, TCommand, TEvent>
        where TState : class
        where TCommand : class
        where TEvent : class
    {
        private readonly IDocumentSession _context;
        private readonly Func<DateTime> _now;
        private readonly Func<TCommand, Func<DateTime>, TState, Task<TEvent[]>> _decider;
        private readonly Func<TState, TEvent, TState> _evolve;

        public CommandHandler(
            IDocumentSession context,
            Func<DateTime> now,
            Func<TCommand, Func<DateTime>, TState, Task<TEvent[]>> decider,
            Func<TState, TEvent, TState> evolve)
        {
            _context = context;
            _now = now;
            _decider = decider;
            _evolve = evolve;
        }

        public async Task Handle(Guid id, TCommand command, CancellationToken ct)
        {
            await _context.GetAndUpdate<TState>(
                id, (current) => _decider(command, _now, current),
                ct);
        }
    }
}
