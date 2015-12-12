using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace CountryApi.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //http://blogs.msdn.com/b/yaohuang1/archive/2012/05/21/asp-net-web-api-generating-a-web-api-help-page-using-apiexplorer.aspx

            var apiExplorer = GlobalConfiguration.Configuration.Services.GetApiExplorer();
            return View(apiExplorer);
        }
    }
}
