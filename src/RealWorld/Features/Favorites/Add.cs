using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RealWorld.Domain;
using RealWorld.Features.Articles;
using RealWorld.Infrastructure;
using RealWorld.Infrastructure.Errors;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace RealWorld.Features.Favorites
{
    public class Add
    {
        public class Command : IRequest<ArticleEnvelope>
        {
            public Command(string slug)
            {
                Slug = slug;
            }

            public string Slug { get; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Slug).NotNull().NotEmpty();
            }
        }

        public class QueryHandler : IRequestHandler<Command, ArticleEnvelope>
        {
            private readonly RealWorldContext _context;
            private readonly ICurrentUserAccessor _currentUserAccessor;

            public QueryHandler(RealWorldContext context, ICurrentUserAccessor currentUserAccessor)
            {
                _context = context;
                _currentUserAccessor = currentUserAccessor;
            }

            public async Task<ArticleEnvelope> Handle(Command message, CancellationToken cancellationToken)
            {
                var article =
                    await _context.Articles.FirstOrDefaultAsync(x => x.Slug == message.Slug, cancellationToken);

                if (article == null) throw new RestException(HttpStatusCode.NotFound);

                var person =
                    await _context.Persons.FirstOrDefaultAsync(
                        x => x.Username == _currentUserAccessor.GetCurrentUsername(), cancellationToken);

                var favorite = await _context.ArticleFavorites.FirstOrDefaultAsync(
                    x => x.ArticleId == article.ArticleId && x.PersonId == person.PersonId, cancellationToken);

                if (favorite == null)
                {
                    favorite = new ArticleFavorite
                    {
                        Article = article,
                        ArticleId = article.ArticleId,
                        Person = person,
                        PersonId = person.PersonId
                    };
                    await _context.ArticleFavorites.AddAsync(favorite, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);
                }

                return new ArticleEnvelope(await _context.Articles.GetAllData()
                    .FirstOrDefaultAsync(x => x.ArticleId == article.ArticleId, cancellationToken));
            }
        }
    }
}