using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RealWorld.Infrastructure;
using RealWorld.Infrastructure.Security;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RealWorld.Features.Users
{
    public class Edit
    {
        public class UserData
        {
            public string Username { get; set; }

            public string Email { get; set; }

            public string Password { get; set; }

            public string Bio { get; set; }

            public string Image { get; set; }
        }

        public class Command : IRequest<UserEnvelope>
        {
            public UserData User { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.User).NotNull();
            }
        }

        public class Handler : IRequestHandler<Command, UserEnvelope>
        {
            private readonly RealWorldContext _db;
            private readonly IPasswordHasher _passwordHasher;
            private readonly ICurrentUserAccessor _currentUserAccessor;

            public Handler(RealWorldContext db, IPasswordHasher passwordHasher, ICurrentUserAccessor currentUserAccessor)
            {
                _db = db;
                _passwordHasher = passwordHasher;
                _currentUserAccessor = currentUserAccessor;
            }

            public async Task<UserEnvelope> Handle(Command request, CancellationToken cancellationToken)
            {
                var currentUsername = _currentUserAccessor.GetCurrentUsername();
                var person = await _db.Persons.Where(x => x.Username == currentUsername).FirstOrDefaultAsync();

                person.Username = request.User.Username ?? person.Username;
                person.Email = request.User.Email ?? person.Email;
                person.Bio = request.User.Bio ?? person.Bio;
                person.Image = request.User.Image ?? person.Image;

                if (!string.IsNullOrWhiteSpace(request.User.Password))
                {
                    var salt = Guid.NewGuid().ToByteArray();
                    person.Hash = _passwordHasher.Hash(request.User.Password, salt);
                    person.Salt = salt;
                }

                await _db.SaveChangesAsync();

                return new UserEnvelope(Mapper.Map<Domain.Person, User>(person));
            }
        }
    }
}
