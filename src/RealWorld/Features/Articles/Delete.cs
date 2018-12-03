using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RealWorld.Infrastructure;
using RealWorld.Infrastructure.Errors;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace RealWorld.Features.Articles
{
    public class Delete
    {
        public class Command : IRequest
        {
            public Command(string slug)
            {
                Slug = slug;
            }

            public string Slug { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Slug).NotNull().NotEmpty();
            }
        }

        public class QueryHandler : IRequestHandler<Command>
        {
            private readonly RealWorldContext _context;

            public QueryHandler(RealWorldContext context)
            {
                _context = context;
            }

            public async Task<Unit> Handle(Command message, CancellationToken cancellationToken)
            {
                var article = await _context.Articles
                    .FirstOrDefaultAsync(x => x.Slug == message.Slug, cancellationToken);

                if (article == null) throw new RestException(HttpStatusCode.NotFound);

                _context.Articles.Remove(article);
                await _context.SaveChangesAsync(cancellationToken);

                return Unit.Value;
            }
        }
    }
}