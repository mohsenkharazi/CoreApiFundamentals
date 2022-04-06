using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCodeCamp.Controllers
{
    [ApiController]
    public class OperationsController : ControllerBase
    {
        private readonly IConfiguration _cfg;

        public OperationsController(IConfiguration cfg)
        {
            this._cfg = cfg;
        }
    }
}
