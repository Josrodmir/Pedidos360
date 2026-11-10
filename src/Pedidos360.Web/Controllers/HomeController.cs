using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Pedidos360.Web.Models;

namespace Pedidos360.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int? id)
    {
        if (id.HasValue)
        {
            Response.StatusCode = id.Value;
        }

        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
