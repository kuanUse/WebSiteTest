using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcTest.Controllers
{
    public class BootstrapTestController : Controller
    {
        // GET: BootstrapTest
        [HttpGet]
        public ActionResult Index()
        {
            return View("BootstrapTest");
        }
    }
}