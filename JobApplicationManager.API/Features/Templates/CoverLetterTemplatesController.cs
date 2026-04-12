using Microsoft.AspNetCore.Mvc;

namespace JobApplicationManager.API.Features.Templates
{
    public class CoverLetterTemplatesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
