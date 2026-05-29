using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MyPetClinic.Models;

namespace MyPetClinic.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult History()
    {
        return View();
    }

    public IActionResult Team()
    {
        return View();
    }

    public IActionResult KhamDieuTri()
    {
        return View();
    }

    public IActionResult TiemPhong()
    {
        return View();
    }

    public IActionResult SpaGrooming()
    {
        return View();
    }

    public IActionResult TinTuc()
    {
        return View();
    }

    public IActionResult SucKhoe()
    {
        return View();
    }

    public IActionResult Contact()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public IActionResult Article(int id = 1)
    {
        ViewBag.ArticleId = id;
        return View();
    }
}
