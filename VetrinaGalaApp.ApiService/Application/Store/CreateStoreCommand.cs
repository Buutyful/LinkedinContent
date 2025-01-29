using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using VetrinaGalaApp.ApiService.EndPoints;
using VetrinaGalaApp.ApiService.Infrastructure;

namespace VetrinaGalaApp.ApiService.Application.Store;

public record CreateStoreCommand(Guid UserId, CreateStoreRequest Request) : IRequest<ErrorOr<AuthenticationResult>>;

//public class CreateStoreCommandHandler(
//    AppDbContext context,
//    RoleManager<IdentityRole> roleManager) : IRequestHandler<CreateStoreCommand, ErrorOr<AuthenticationResult>>
//{
//    private readonly RoleManager<IdentityRole> _roleManager = roleManager;
//    private readonly AppDbContext _appDbContext = context;
//    public Task<ErrorOr<AuthenticationResult>> Handle(CreateStoreCommand request, CancellationToken cancellationToken)
//    {
//        //create store
//        //add role
//        //refresh token
        
//    }
//}

