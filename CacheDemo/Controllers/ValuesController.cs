using System.Collections.Generic;
using System.Web.Http;
using CacheDemo.Data.Contracts;

namespace CacheDemo.Controllers
{
    public class ValuesController : ApiBaseController
    {
        public ValuesController(IUnitOfWork uow)
        {
            UnitOfWork = uow;
        }

        public ValuesController()
        {
        }

        // GET api/values
        public IEnumerable<string> Get()
        {
            var a = UnitOfWork.Portals.GetById(1);
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
