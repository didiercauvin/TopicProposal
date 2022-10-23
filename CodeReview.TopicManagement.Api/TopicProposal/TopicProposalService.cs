using CodeReview.TopicManagement.Api.TopicProposal;
using Marten;

namespace CodeReview.TopicManagement.Api.TopicProposal
{
    public record ProposeTopic(Guid TopicId, string Label, string Description, string Requester, DateTimeOffset ScheduleDate);
    public record ScheduleTopic(Guid TopicId);
    public record ProposeRescheduleTopic(Guid TopicId, DateTimeOffset RescheduleDate);
    public record AcceptTopicReschedule(Guid TopicId);
    public record RefuseTopicReschedule(Guid TopicId);

    public static class TopicProposalService
    {
        public static TopicRequested Handle(ProposeTopic command) =>
            new TopicRequested(command.TopicId, command.Label, command.Description,
                command.Requester, command.ScheduleDate, DateTimeOffset.UtcNow);

        public static TopicScheduled Handle(CodeReviewTopic topic, ScheduleTopic command) =>
            new TopicScheduled(command.TopicId, DateTimeOffset.UtcNow);

        public static TopicRescheduleProposed Handle(CodeReviewTopic topic, ProposeRescheduleTopic command) =>
            new TopicRescheduleProposed(command.TopicId, command.RescheduleDate, DateTimeOffset.UtcNow);

        public static TopicRescheduleAccepted Handle(CodeReviewTopic topic, AcceptTopicReschedule command) =>
            new TopicRescheduleAccepted(command.TopicId, DateTimeOffset.UtcNow);

        public static TopicRescheduleRefused Handle(CodeReviewTopic topic, RefuseTopicReschedule command) =>
            new TopicRescheduleRefused(command.TopicId, DateTimeOffset.UtcNow);
    }
}
