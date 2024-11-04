// Copyright © myCSharp.de - all rights reserved

using Microsoft.AspNetCore.Mvc;

namespace MyCSharp.HttpClientHints.Samples.AspNetCoreMvc.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }
}
