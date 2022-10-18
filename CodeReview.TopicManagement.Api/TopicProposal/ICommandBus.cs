namespace CodeReview.TopicManagement.Api.TopicProposal
{
    public interface ICommandBus
    {
        Task Send<TCommand>(TCommand command, CancellationToken ct) where TCommand: class;
    }

}
