using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SPM.Http.SelfUpdateHost.Controllers
{
    public class HomeController : Controller
    {
        private readonly StoreService storeService;

        public HomeController(StoreService storeService)
        {
            this.storeService = storeService;
        }

        public async Task<IActionResult> Index()
        {
            List<VersionRow> lastVersions = await storeService.GetLastVersions(10);
            return View(lastVersions);
        }
    }
}