using System.Web.Http;
using CacheDemo.Data.Contracts;

namespace CacheDemo.Controllers
{
    public class ApiBaseController : ApiController
    {
        public ApiBaseController()
        {

        }

        public IUnitOfWork UnitOfWork { get; set; }
    }
}