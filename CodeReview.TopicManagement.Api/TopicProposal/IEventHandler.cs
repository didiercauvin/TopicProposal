namespace CodeReview.TopicManagement.Api.TopicProposal
{
    public interface IEventHandler<in TEvent>
    {
        Task Handle(TEvent @event, CancellationToken ct);
    }

    public interface ICommandHandler<in TCommand>
    {
        Task Handle(TCommand command, CancellationToken ct);
    }

}
