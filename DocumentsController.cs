using Evolution.Internet.Filters;
using Evolution.Internet.Logic;
using Evolution.Internet.Logic.Commands;
using Evolution.Internet.Logic.Model;
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
    [Route("api/documents")]
    public class DocumentsController : Controller
    {
        private readonly IMediator _mediator;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mediator"></param>
        public DocumentsController(IMediator mediator)
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
        [SwaggerResponse(200, Type = typeof(Document[]))]
        [Produces("application/json", "application/xml", "text/xml")]
        public async Task<IActionResult> Query([FromQuery]int? page = 1, [FromQuery]int? pageSize = 10, [FromQuery]string filter = null, [FromQuery]string orderBy = "CreateDate DESC")
        {
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id:guid}")]
        [SwaggerResponse(200, Type = typeof(Document))]
        [Produces("application/json", "application/xml", "text/xml")]
        public async Task<IActionResult> Get(string id)
        {
            var sw = new Stopwatch();
            sw.Start();
            var data = await _mediator.Send(new SingleQuery<Document>(id));
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
        /// Download binary file content of document
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id:guid}.bin")]
        [SwaggerResponse(200, Type = typeof(FileStreamResult))]
        public async Task< IActionResult> DownloadBin(string id)
        {
            var result = await _mediator.Send(new SingleQuery<DownloadFileData>(id));
            return File(result.Content, result.ContentType, result.FileName);
        }

        /// <summary>
        /// Download binary file content as pdf
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id:guid}.pdf")]
        [SwaggerResponse(200, Type = typeof(FileStreamResult))]
        public async Task<IActionResult> DownloadPdf(string id)
        {
            var metadata = await _mediator.Send(new SingleQuery<FileMetadata>(id));
            if(metadata.Extension.ToLower() == ".pdf")
            {
                var result = await _mediator.Send(new SingleQuery<DownloadFileData>(id));
                return File(result.Content, result.ContentType, result.FileName);
            }
            else
            {
                await _mediator.Send(new PdfConversionCommand(metadata));
                var result = await _mediator.Send(new SingleQuery<DownloadPdfFileData>(id));
                return File(result.Content, result.ContentType, result.FileName);
            }
        }
    }
}

