using Evolution.Internet.Filters;
using Evolution.Internet.Logic;
using Evolution.Internet.Logic.Queries;
using Evolution.Internet.Logic.ViewModel;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Evolution.Internet.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ServiceFilter(typeof(ApiExceptionFilter))]
    [Route("api/cases")]
    public class CasesController : Controller
    {
        private readonly IMediator _mediator;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mediator"></param>
        public CasesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="page">Default = 1</param>
        /// <param name="pageSize">Default = 10</param>
        /// <param name="filter"></param>
        /// <param name="orderBy">Defalt = 'CreateDate DESC'</param>
        /// <returns></returns>
        [HttpGet]
        [SwaggerResponse(200, Type = typeof(Case[]))]
        [Produces("application/json", "application/xml", "text/xml")]
        public async Task<IActionResult> Query([FromQuery]int? page = 1, [FromQuery]int? pageSize = 10, [FromQuery]string filter = null, [FromQuery]string orderBy = "CreateDate DESC")
        {
            var sw = new Stopwatch();
            sw.Start();
            var result = await _mediator.Send(new PagedQuery<Case>((int)page, (int)pageSize, filter, orderBy));
            sw.Stop();

            var pagingMeta = PagingMeta.Create(result.TotalRows, (int)pageSize, (int)page);
            pagingMeta.Elapsed = sw.Elapsed;

            return Ok(new
            {
                meta = pagingMeta,
                data = result.Data
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id:guid}")]
        [SwaggerResponse(200, Type = typeof(Case))]
        [Produces("application/json", "application/xml", "text/xml")]
        public async Task<IActionResult> Get(string id)
        {
            var sw = new Stopwatch();
            sw.Start();
            var data = await _mediator.Send(new SingleQuery<Case>(id));
            sw.Stop();

            var meta = new TimeSpanMeta
            {
                Elapsed = sw.Elapsed
            };

            return Ok(new
            {
                meta,
                data
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="page">Default = 1</param>
        /// <param name="pageSize">Default = 10</param>
        /// <param name="orderBy">Defalt = 'CreateDate DESC'</param>
        /// <returns></returns>
        [HttpGet("{id:guid}/documents")]
        [SwaggerResponse(200, Type = typeof(Document[]))]
        [Produces("application/json", "application/xml", "text/xml")]
        public async Task<IActionResult> GetDocuments(string id, [FromQuery]int? page = 1, [FromQuery]int? pageSize = 10, [FromQuery]string orderBy = "CreateDate DESC")
        {
            var filter = $"CaseId:{id}";
            var sw = new Stopwatch();
            sw.Start();
            var result = await _mediator.Send(new PagedQuery<Document>((int)page, (int)pageSize, filter, orderBy));
            sw.Stop();

            var pagingMeta = PagingMeta.Create(result.TotalRows, (int)pageSize, (int)page);
            pagingMeta.Elapsed = sw.Elapsed;

            return Ok(new
            {
                meta = pagingMeta,
                data = result.Data
            });
        }
    }
}


