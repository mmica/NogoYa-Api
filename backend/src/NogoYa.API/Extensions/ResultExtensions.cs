using Microsoft.AspNetCore.Mvc;
using NogoYa.Application.Common;

namespace NogoYa.API.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess) return new OkObjectResult(result.Value);
        return ToProblem(result.Error!, result.ErrorCode);
    }

    public static IActionResult ToActionResult(this Result result)
    {
        if (result.IsSuccess) return new NoContentResult();
        return ToProblem(result.Error!, result.ErrorCode);
    }

    private static ObjectResult ToProblem(string error, string? code)
    {
        var status = code switch
        {
            "STORE_NOT_FOUND" or "PRODUCT_NOT_FOUND" or "ORDER_NOT_FOUND" => StatusCodes.Status404NotFound,
            "SLUG_ALREADY_EXISTS" => StatusCodes.Status409Conflict,
            "INVALID_STATUS_TRANSITION" or "EMPTY_ORDER" or "INVALID_DISCOUNT" or "INVALID_PRICE"
                => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status400BadRequest
        };
        return new ObjectResult(new { status, error, code }) { StatusCode = status };
    }
}
