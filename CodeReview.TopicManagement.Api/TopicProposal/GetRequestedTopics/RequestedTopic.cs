using Marten.Events.Aggregation;

namespace CodeReview.TopicManagement.Api.TopicProposal.GetRequestedTopics
{
    public record RequestedTopic(Guid Id, string Label, string Description, string Requester, 
        DateTimeOffset? ScheduleDate, CodeReviewTopicStatus Status);

    public class RequestedTopicHistoryTransformation : SingleStreamAggregation<RequestedTopic>
    {
        public static RequestedTopic Create(TopicRequested requested) =>
            new(requested.Id, requested.Label, requested.Description, requested.Requester, 
                requested.ScheduleDate, CodeReviewTopicStatus.Requested);

        public RequestedTopic Apply(TopicScheduled scheduled, RequestedTopic current) =>
            current with { Status = CodeReviewTopicStatus.Scheduled };

        public RequestedTopic Apply(TopicRescheduleProposed proposed, RequestedTopic current) =>
            current with { Status = CodeReviewTopicStatus.RescheduleProposed };

        public RequestedTopic Apply(TopicRescheduleAccepted accepted, RequestedTopic current) =>
            current with { Status = CodeReviewTopicStatus.Scheduled };

        public RequestedTopic Apply(TopicRescheduleRefused refused, RequestedTopic current) =>
            current with { Status = CodeReviewTopicStatus.Refused };
    }
}
