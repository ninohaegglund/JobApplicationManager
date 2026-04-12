using Microsoft.AspNetCore.Mvc;

namespace JobApplicationManager.API.Features.TextBlocks
{
    public class TextBlocksController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
