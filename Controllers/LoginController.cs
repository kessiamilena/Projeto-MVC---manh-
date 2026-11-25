
using Microsoft.AspNetCore.Mvc;

namespace Bibliotec_MVC___manha.Controllers;

public class LoginController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}