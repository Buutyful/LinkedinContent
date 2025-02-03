using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using VetrinaGalaApp.ApiService.Application.Common.Security;
using VetrinaGalaApp.ApiService.Domain;
using VetrinaGalaApp.ApiService.EndPoints;
using VetrinaGalaApp.ApiService.Infrastructure;

namespace VetrinaGalaApp.ApiService.Application.StoreUseCases;

public record CreateStoreCommand(Guid UserId, CreateStoreRequest Request) : IRequest<ErrorOr<AuthenticationResult>>;

public class CreateStoreCommandHandler(
    AppDbContext context,
    UserManager<User> manager,
    IJwtTokenGenerator tokenGenerator) : IRequestHandler<CreateStoreCommand, ErrorOr<AuthenticationResult>>
{
    private readonly UserManager<User> _manager = manager;
    private readonly AppDbContext _appDbContext = context;
    private readonly IJwtTokenGenerator _jwtTokenGenerator = tokenGenerator;
    public async Task<ErrorOr<AuthenticationResult>> Handle(CreateStoreCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await _appDbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var user = await _appDbContext.Users.FindAsync([request.UserId], cancellationToken);
            if (user is null)
                return Error.NotFound();

            user.UserType = UserType.StoreOwner;

            var store = new Store
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Name = request.Request.Name,
                Description = request.Request.Description,
            };

            user.Store = store;
            await _appDbContext.Stores.AddAsync(store, cancellationToken);
            await _appDbContext.SaveChangesAsync(cancellationToken);

            var roleResult = await _manager.AddToRoleAsync(user, RoleConstants.StoreOwner);

            if (!roleResult.Succeeded)
                return Error.Failure(metadata: roleResult.Errors
                    .GroupBy(e => e.Code)
                    .ToDictionary(
                    g => g.Key,
                    g => (object)g.Select(e => e.Description).ToArray()));

            await transaction.CommitAsync(cancellationToken);

            var token = _jwtTokenGenerator.GenerateToken(user);
            return new AuthenticationResult(user.Id, user.Email!, token);
        }
        catch(Exception e)
        {
            return Error.Failure(description: e.Message);
        }
    }
}

