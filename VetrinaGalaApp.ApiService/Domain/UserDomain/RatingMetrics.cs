namespace VetrinaGalaApp.ApiService.Domain.UserDomain;

public class RatingMetrics
{
    public const int DefaultInitialMmr = 1500;
    public const int DefaultKFactor = 32;

    public int MMR { get; private set; }
    public int LikeCount { get; private set; }
    public int DislikeCount { get; private set; }
    public double PositiveRatio =>
        LikeCount + DislikeCount == 0 ?
        0.5 : (double)LikeCount / (LikeCount + DislikeCount);

    public RatingMetrics(int likeCount = 0, int dislikeCount = 0, int initialMMR = DefaultInitialMmr)
    {
        MMR = initialMMR;
        LikeCount = likeCount;
        DislikeCount = dislikeCount;
    }

    public void AddLike()
    {
        LikeCount++;
        MMR = CalculateNewMmr();
    }

    public void AddDislike()
    {
        DislikeCount++;
        MMR = CalculateNewMmr();
    }  
    private int CalculateNewMmr()
    {
        double expectedScore = 1.0 / (1.0 + Math.Pow(10, (DefaultInitialMmr - MMR) / 400.0));       

        var newMmr = MMR + (int)(DefaultKFactor * (PositiveRatio - expectedScore));
        return Math.Max(0, newMmr);
    }
}
