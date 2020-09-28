using Evolution.Internet.Filters;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Evolution.Internet.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ServiceFilter(typeof(ApiExceptionFilter))]
    [Route("api/params")]
    public class ParametersController : Controller
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly string _schemasPath;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hostingEnvironment"></param>
        public ParametersController(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            _schemasPath = _hostingEnvironment.ContentRootPath + "\\schemas";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("document")]
        public IActionResult DocumentParams()
        {
            var json = GetJsonSchema("documents.json");
            return Ok(json);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("case")]
        public IActionResult CasesParams()
        {
            var json = GetJsonSchema("cases.json");
            return Ok(json);
        }

        private object GetJsonSchema(string schemaFileName)
        {
            var json = System.IO.File.ReadAllText(System.IO.Path.Combine(_schemasPath, schemaFileName));
            return JObject.Parse(json);
        }
    }
}
