using Microsoft.AspNetCore.Mvc;

namespace CourseManagementMvc.Controllers;

public abstract class AppController : Controller
{
    protected IActionResult Missing(string message)
    {
        Response.StatusCode = StatusCodes.Status404NotFound;
        return View("~/Views/Shared/NotFoundFriendly.cshtml", (object)message);
    }
}
