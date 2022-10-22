namespace CodeReview.TopicManagement.Api.TopicProposal
{
    public record TopicRequested(Guid Id, string Label, string Description, string Requester, 
        DateTimeOffset ScheduleDate, DateTimeOffset RequestedAt);
    public record TopicScheduled(Guid Id, DateTimeOffset ScheduledAt);
    public record TopicRescheduleProposed(Guid Id, DateTimeOffset NewScheduleDate, DateTimeOffset ProposedAt);
    public record TopicRescheduleAccepted(Guid Id, DateTimeOffset AcceptedAt);
    public record TopicRescheduleRefused(Guid Id, DateTimeOffset RefusedAt);

    public enum CodeReviewTopicStatus
    {
        Requested,
        Scheduled,
        RescheduleProposed,
        Refused
    }

    public record CodeReviewTopic(
        Guid Id,
        string Label,
        string Description,
        string Author,
        DateTimeOffset ScheduleDate,
        CodeReviewTopicStatus Status = CodeReviewTopicStatus.Requested)
    {
        public static CodeReviewTopic Create(TopicRequested requested) =>
            new(requested.Id, requested.Label, requested.Label, requested.Requester, requested.ScheduleDate);

        public CodeReviewTopic Apply(TopicScheduled scheduled) =>
            this with { Status = CodeReviewTopicStatus.Scheduled };

        public CodeReviewTopic Apply(TopicRescheduleProposed proposed) =>
            this with { Status = CodeReviewTopicStatus.RescheduleProposed };

        public CodeReviewTopic Apply(TopicRescheduleAccepted accepted) =>
            this with { Status = CodeReviewTopicStatus.Scheduled };

        public CodeReviewTopic Apply(TopicRescheduleRefused refused) =>
            this with { Status = CodeReviewTopicStatus.Refused };
    }
}
