using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using TempleVolunteerClient.Common;
using TempleVolunteerClient.ViewModels;

namespace TempleVolunteerClient.Controllers
{
    public class HomeController : CustomController
    {
        public HomeController(IHttpContextAccessor httpContextAccessor, IOptions<AppSettings> AppSettings)
            : base(httpContextAccessor, AppSettings)
        {
        }

        public async Task<IActionResult> Index()
        {
            LoginViewModel viewModel = new LoginViewModel();
            viewModel.EmailAddress = "gerryhazelton@gmail.com";
            viewModel.Password = "Mataji99#";
            viewModel.TemplePropertyList = await this.GetTempleProperties(true, false);

            return View(viewModel);
        }

        public IActionResult AdminMenu()
        {
            return View();
        }

        public IActionResult Menu()
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
    }
}