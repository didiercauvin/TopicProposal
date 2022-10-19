namespace CodeReview.TopicManagement.Api.TopicProposal
{
    public record TopicRequested(Guid Id, string Label, string Description, string Requester);
    public record TopicScheduled(Guid Id, DateTimeOffset ScheduleDate);

    public enum CodeReviewTopicStatus
    {
        Requested,
        Scheduled
    }

    public record CodeReviewTopic(
        Guid Id,
        string Label,
        string Description,
        string Author,
        CodeReviewTopicStatus Status = CodeReviewTopicStatus.Requested)
    {
        public static CodeReviewTopic Create(TopicRequested requested) =>
            new(requested.Id, requested.Label, requested.Label, requested.Requester);

        public CodeReviewTopic Apply(TopicScheduled categorised) =>
            this with { Status = CodeReviewTopicStatus.Scheduled };
    }
}
