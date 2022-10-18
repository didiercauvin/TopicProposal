﻿using Marten.Events.Aggregation;

namespace CodeReview.TopicManagement.Api.TopicProposal.GetRequestedTopics
{
    public record RequestedTopic(Guid Id, string Label, string Description, string Requester);

    public class RequestedTopicHistoryTransformation : SingleStreamAggregation<RequestedTopic>
    {
        public static RequestedTopic Create(TopicRequested requested) =>
            new(requested.Id, requested.Label, requested.Description, requested.Requester);
    }
}