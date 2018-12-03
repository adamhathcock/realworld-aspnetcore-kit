using MediatR;
using Microsoft.EntityFrameworkCore;
using RealWorld.Infrastructure;
using RealWorld.Infrastructure.Errors;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace RealWorld.Features.Comments
{
    public class List
    {
        public class Query : IRequest<CommentsEnvelope>
        {
            public Query(string slug)
            {
                Slug = slug;
            }

            public string Slug { get; }
        }

        public class QueryHandler : IRequestHandler<Query, CommentsEnvelope>
        {
            private readonly RealWorldContext _context;

            public QueryHandler(RealWorldContext context)
            {
                _context = context;
            }

            public async Task<CommentsEnvelope> Handle(Query message, CancellationToken cancellationToken)
            {
                var article = await _context.Articles
                    .Include(x => x.Comments)
                    .FirstOrDefaultAsync(x => x.Slug == message.Slug, cancellationToken);

                if (article == null)
                {
                    throw new RestException(HttpStatusCode.NotFound);
                }

                return new CommentsEnvelope(article.Comments);
            }
        }
    }
}