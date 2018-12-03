using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealWorld.Features.Articles;
using RealWorld.Infrastructure.Security;
using System.Threading.Tasks;

namespace RealWorld.Features.Favorites
{
    [Route("articles")]
    public class FavoritesController : Controller
    {
        private readonly IMediator _mediator;

        public FavoritesController(IMediator mediator)
        {
            _mediator = mediator;
        }
        
        [HttpPost("{slug}/favorite")]
        [Authorize(AuthenticationSchemes = JwtIssuerOptions.Scheme)]
        public async Task<ArticleEnvelope> FavoriteAdd(string slug)
        {
            return await _mediator.Send(new Add.Command(slug));
        }

        [HttpDelete("{slug}/favorite")]
        [Authorize(AuthenticationSchemes = JwtIssuerOptions.Scheme)]
        public async Task FavoriteDelete(string slug)
        {
            await _mediator.Send(new Delete.Command(slug));
        }
    }
}