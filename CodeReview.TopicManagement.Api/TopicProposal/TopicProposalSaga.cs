namespace CodeReview.TopicManagement.Api.TopicProposal
{

    public class TopicProposalSaga : IEventHandler<TopicRequested>
    {
        private readonly ICommandBus _commandBus;

        public TopicProposalSaga(ICommandBus commandBus)
        {
            _commandBus = commandBus;
        }

        public Task Handle(TopicRequested requested, CancellationToken cancellationToken)
        {
            // what to do here?
            //TopicProposalService.Handle(new ScheduleTopic(requested.Id));

            return Task.CompletedTask;
        }
    }

}
