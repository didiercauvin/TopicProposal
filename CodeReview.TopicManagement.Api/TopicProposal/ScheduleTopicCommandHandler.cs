using Marten;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeReview.TopicManagement.Api.TopicProposal
{
    public class CommandHandlers : 
        ICommandHandler<ProposeTopic>,
        ICommandHandler<ScheduleTopic>
    {
        private readonly IDocumentSession _documentSession;

        public CommandHandlers(IDocumentSession documentSession)
        {
            _documentSession = documentSession;
        }

        public async Task Handle(ProposeTopic command, CancellationToken ct)
        {
            await _documentSession.Save<CodeReviewTopic>(command.TopicId,
                TopicProposalService.Handle(command), ct);
        }

        public async Task Handle(ScheduleTopic command, CancellationToken ct)
        {
            await _documentSession.GetAndUpdate<CodeReviewTopic>(command.TopicId, topic =>
                TopicProposalService.Handle(topic, command), ct);
        }
    }
}
