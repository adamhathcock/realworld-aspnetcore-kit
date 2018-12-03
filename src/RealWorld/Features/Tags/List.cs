using MediatR;
using Microsoft.EntityFrameworkCore;
using RealWorld.Infrastructure;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RealWorld.Features.Tags
{
    public class List
    {
        public class Query : IRequest<TagsEnvelope>
        {
        }

        public class QueryHandler : IRequestHandler<Query, TagsEnvelope>
        {
            private readonly RealWorldContext _context;

            public QueryHandler(RealWorldContext context)
            {
                _context = context;
            }

            public async Task<TagsEnvelope> Handle(Query request, CancellationToken cancellationToken)
            {
                var tags = await _context.Tags.OrderBy(x => x.TagId).AsNoTracking().ToListAsync();
                return new TagsEnvelope()
                {
                    Tags = tags.Select(x => x.TagId).ToList()
                };
            }
        }
    }
}