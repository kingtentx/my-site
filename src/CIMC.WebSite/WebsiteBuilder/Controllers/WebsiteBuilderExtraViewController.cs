using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MySite.Web.WebsiteBuilder.Controllers
{
    [Authorize]
    [Route("WebsiteBuilder")]
    public class WebsiteBuilderExtraViewController : Controller
    {
        [HttpGet("Navigation")]
        public IActionResult Navigation() => View("~/Views/WebsiteBuilder/Navigation.cshtml");

        [HttpGet("Banners")]
        public IActionResult Banners() => View("~/Views/WebsiteBuilder/Banners.cshtml");

        [HttpGet("Footer")]
        public IActionResult Footer() => View("~/Views/WebsiteBuilder/Footer.cshtml");

        [HttpGet("Materials")]
        public IActionResult Materials() => View("~/Views/WebsiteBuilder/Materials.cshtml");

        [HttpGet("Categories")]
        public IActionResult Categories() => View("~/Views/WebsiteBuilder/Categories.cshtml");

        [HttpGet("Applications")]
        public IActionResult Applications() => View("~/Views/WebsiteBuilder/Applications.cshtml");
    }
}