using Evolution.Internet.Filters;
using Evolution.Internet.Logic.Queries;
using Evolution.Internet.Logic.ViewModel;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
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
    [Route("api")]
    public class OrganisationController : Controller
    {
        private readonly IMediator _mediator;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mediator"></param>
        public OrganisationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("units")]
        [SwaggerResponse(200, Type = typeof(CodeName[]))]
        [Produces("application/json", "application/xml", "text/xml")]
        public async Task<IActionResult> GetUnits()
        {
            var data = await _mediator.Send(new UnitsQuery());
            return Ok(new
            {
                data
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unitCode"></param>
        /// <returns></returns>
        [HttpGet("departments/{unitCode}")]
        [SwaggerResponse(200, Type = typeof(CodeName[]))]
        [Produces("application/json", "application/xml", "text/xml")]
        public async Task<IActionResult> GetDepartments(string unitCode)
        {
            var data = await _mediator.Send(new DepartmentsQuery(unitCode));
            return Ok(new
            {
                data
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unitCode"></param>
        /// <returns></returns>
        [HttpGet("casetypes/{unitCode}")]
        [SwaggerResponse(200, Type = typeof(CodeName[]))]
        [Produces("application/json", "application/xml", "text/xml")]
        public async Task<IActionResult> GetCaseTypes(string unitCode)
        {
            var data = await _mediator.Send(new CaseTypesQuery(unitCode));
            return Ok(new
            {
                data
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unitCode"></param>
        /// <returns></returns>
        [HttpGet("documenttypes/{unitCode}")]
        [SwaggerResponse(200, Type = typeof(CodeName[]))]
        [Produces("application/json", "application/xml", "text/xml")]
        public async Task<IActionResult> GetDocumentTypes(string unitCode)
        {
            var data = await _mediator.Send(new DocumentTypesQuery(unitCode));
            return Ok(new
            {
                data
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("organs")]
        [SwaggerResponse(200, Type = typeof(IdName[]))]
        [Produces("application/json", "application/xml", "text/xml")]
        public async Task<IActionResult> GetOrgans()
        {
            var data = await _mediator.Send(new OrgansQuery());
            return Ok(new
            {
                data
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("politicalAuthorities")]
        [SwaggerResponse(200, Type = typeof(IdName[]))]
        [Produces("application/json", "application/xml", "text/xml")]
        public async Task<IActionResult> GetPoliticalAuthorities()
        {
            var data = await _mediator.Send(new PoliticalAuthoritiesQuery());
            return Ok(new
            {
                data
            });
        }
    }
}
