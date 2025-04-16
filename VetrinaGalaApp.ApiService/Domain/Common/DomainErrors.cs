using ErrorOr;
namespace VetrinaGalaApp.ApiService.Domain.Common;

public static partial class DomainErrors
{
    public static class UserFeedErrors
    {
        public static Error FeedIsEmpty => Error.Custom(0, code: "Feed_Empty", description: "User feed is empty");
    }
}