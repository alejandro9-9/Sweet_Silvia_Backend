using Microsoft.AspNetCore.Mvc;
using Sweet_Silvia_Application.Common;
using Sweet_Silvia_Domain.Abstractions;

namespace Sweet_Silvia_Api.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult(this ControllerBase controller, Result result)
    {
        return result.IsSuccess
            ? controller.NoContent()
            : Failure(controller, result.Error);
    }

    public static IActionResult ToActionResult<TValue>(this ControllerBase controller, Result<TValue> result)
    {
        return result.IsSuccess
            ? controller.Ok(result.Value)
            : Failure(controller, result.Error);
    }

    public static IActionResult ToCreatedResult(this ControllerBase controller, Result<Guid> result)
    {
        return result.IsSuccess
            ? controller.Ok(new { id = result.Value })
            : Failure(controller, result.Error);
    }

    private static IActionResult Failure(ControllerBase controller, Error error)
    {
        var userFacingError = UserFacingErrorMessages.Localize(error);
        return controller.StatusCode(userFacingError.StatusCode ?? StatusCodes.Status400BadRequest, userFacingError);
    }
}
