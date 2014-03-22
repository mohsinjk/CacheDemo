using CacheDemo.Data.Contracts;

namespace CacheDemo.Tests.Controllers
{
    public class BaseTestClass
    {
        public BaseTestClass(IUnitOfWork uow)
        {
            IocConfig.RegisterIoc();
            UnitOfWork = uow;
        }

        public IUnitOfWork UnitOfWork { get; set; }
    }
}