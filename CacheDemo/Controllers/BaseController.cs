using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CacheDemo.Data.Contracts;

namespace CacheDemo.Controllers
{
    public class BaseController:Controller
    {
        public IUnitOfWork UnitOfWork { get; set; }
    }
}