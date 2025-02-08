using ErrorOr;

namespace VetrinaGalaApp.ApiService.EndPoints;

public static class ResultExtentions
{
    public static IResult ToResult(this List<Error> errors) =>
           errors switch
           {
               null => Results.Problem(),
               [] => Results.Problem(),
               [var error] => MapToResult(error),
               [.. var err] when err.All(e => e.Type == ErrorType.Validation) =>
                   Results.ValidationProblem(
                       err.GroupBy(e => e.Code)
                          .ToDictionary(
                               g => g.Key,
                               g => g.Select(e => e.Description).ToArray())),
               [.. var err] => MapToResult(err[0])
           };

    private static IResult MapToResult(Error error) =>
        error.Type switch
        {
            ErrorType.Unauthorized => Results.Unauthorized(),
            ErrorType.NotFound => Results.NotFound(),
            ErrorType.Forbidden => Results.Forbid(),
            ErrorType.Validation => Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    { error.Code, new[] { error.Description } }
                }),
            ErrorType.Conflict => Results.Conflict(error.Description),
            _ => Results.Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: error.Description)
        };
}

