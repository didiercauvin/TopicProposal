namespace CodeReview.TopicManagement.Api.TopicProposal
{
    public interface IEventHandler<in TEvent>
    {
        Task Handle(TEvent @event, CancellationToken ct);
    }

}
