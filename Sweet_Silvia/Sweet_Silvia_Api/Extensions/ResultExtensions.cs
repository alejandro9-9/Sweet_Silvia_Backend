using Microsoft.AspNetCore.Mvc;
using Sweet_Silvia_Domain.Abstractions;

namespace Sweet_Silvia_Api.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult(this ControllerBase controller, Result result)
    {
        return result.IsSuccess
            ? controller.NoContent()
            : controller.BadRequest(result.Error);
    }

    public static IActionResult ToActionResult<TValue>(this ControllerBase controller, Result<TValue> result)
    {
        return result.IsSuccess
            ? controller.Ok(result.Value)
            : controller.BadRequest(result.Error);
    }

    public static IActionResult ToCreatedResult(this ControllerBase controller, Result<Guid> result)
    {
        return result.IsSuccess
            ? controller.Ok(new { id = result.Value })
            : controller.BadRequest(result.Error);
    }
}
