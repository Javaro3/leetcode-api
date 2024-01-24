using Microsoft.AspNetCore.Mvc;
using LeetCodeApi.Service;

namespace LeetCodeApi.Controllers;

public class StatsController : Controller
{
    private readonly StatsService _statsService;

    public StatsController(StatsService statsService)
    {
        _statsService = statsService;
    }

    public IActionResult Index(string username)
    {
        try{
            var model = _statsService.GetStats(username);
            string svg = _statsService.ConvertModelToSvg(model);
            return Content(svg, "image/svg+xml");
        }
        catch(ArgumentException e){
            return Content(e.Message);
        }
        
    }
}
