using ErrorOr;
using System.ComponentModel.DataAnnotations;
using VetrinaGalaApp.ApiService.Domain.Common;
using static VetrinaGalaApp.ApiService.Domain.UserDomain.DomainErrors;
namespace VetrinaGalaApp.ApiService.Domain.UserDomain;



public class BaseUser : Entity
{   
    public UserType UserType { get; }

    [EmailAddress]
    public string Email { get; }

    private BaseUser(Guid id, UserType userType, string email) : base(id) =>
        (UserType, Email) = (userType, email);
    public static BaseUser CreateNew(string email) =>
        new(Guid.NewGuid(), UserType.User, email);
    public static BaseUser Create(Guid id, string email) =>
        new(id, UserType.User, email);

}

public class UserFeedQueue
{
    private readonly PriorityQueue<DomainItem, int> _swipeQueue = new();
    private DomainItem? _currentItem;

    public UserFeedQueue(List<DomainItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        _swipeQueue.EnqueueRange(items.Select(item => (item, -item.RatingMetrics.MMR)));
    }

    public ErrorOr<Success> Swipe(bool like)
    {
        if (_currentItem is null)
        {
            var nextItemResult = GetNextItem();
            if (nextItemResult.IsError)
                return nextItemResult.FirstError;

            _currentItem = nextItemResult.Value;
        }

        if (like)
        {
            _currentItem.RatingMetrics.AddLike();
        }
        else
        {
            _currentItem.RatingMetrics.AddDislike();
        }

        _currentItem = null;
        return Result.Success;
    }

    public ErrorOr<DomainItem> GetNextItem()
    {
        if (_currentItem is not null)
            return _currentItem;

        if (_swipeQueue.Count == 0)
            return UserFeedErrors.FeedIsEmpty;

        _currentItem = _swipeQueue.Dequeue();
        return _currentItem;
    }
}

public static partial class DomainErrors
{
    public static class UserFeedErrors
    {
        public static Error FeedIsEmpty => Error.Custom(0, code: "Feed_Empty", description: "User feed is empty");
    }
}