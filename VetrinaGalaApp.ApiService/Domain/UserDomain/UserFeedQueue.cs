using ErrorOr;
using static VetrinaGalaApp.ApiService.Domain.Common.DomainErrors;
namespace VetrinaGalaApp.ApiService.Domain.UserDomain;

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
