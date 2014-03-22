using System.Web.Mvc;
using CacheDemo.Data.Contracts;

namespace CacheDemo.Controllers
{
    public class HomeController : Controller
    {
        //public HomeController(IUnitOfWork uow)
        //{
        //    UnitOfWork = uow;
        //}

        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }
    }
}
