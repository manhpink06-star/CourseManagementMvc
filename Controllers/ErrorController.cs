using Microsoft.AspNetCore.Mvc;

namespace CourseManagementMvc.Controllers;

public class ErrorController : Controller
{
    [Route("/Error")]
    public IActionResult Index() => View();
}
