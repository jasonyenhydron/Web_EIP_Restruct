
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;

namespace Web_EIP_Restruct.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View("Index");
        }

        public IActionResult Dashboard()
        {
            return RenderDashboard("Dashboard");
        }

        public IActionResult Board()
        {
            return RenderDashboard("~/Views/Dashboard/Board.cshtml");
        }

        public IActionResult SigonCenter()
        {
            return RenderDashboard("~/Views/Dashboard/sigoncenter.cshtml");
        }

        public IActionResult Settings()
        {
            return RenderDashboard("~/Views/Dashboard/Settings.cshtml");
        }

        private IActionResult RenderDashboard(string viewName)
        {
            var username = HttpContext.Session.GetString("username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Username = username;
            ViewBag.UserName = HttpContext.Session.GetString("user_name");
            ViewBag.TodayDate = DateTime.Today.ToString("yyyy-MM-dd");

            return View(viewName);
        }
    }
}


