using BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;
using ModelLayer.Models;
using System.Threading.Tasks;

namespace Insurance.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SchemeController : ControllerBase
    {
        private readonly ISchemeBL _schemeBL;

        public SchemeController(ISchemeBL schemeBL)
        {
            _schemeBL = schemeBL;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateScheme([FromBody] SchemeModel model)
        {
            var result = await _schemeBL.CreateSchemeAsync(model);
            return Ok(new { Success = true, Message = "Scheme created", Data = result });
        }

        [HttpGet("byplan/{planId}")]
        public async Task<IActionResult> GetSchemes(int planId)
        {
            var result = await _schemeBL.GetSchemesByPlanIdAsync(planId);
            return Ok(new { Success = true, Data = result });
        }
        [HttpGet("{schemeId}")]
        public async Task<IActionResult> GetSchemeById(int schemeId)
        {
            var result = await _schemeBL.GetSchemeByIdAsync(schemeId);
            if (result == null)
                return NotFound(new { Success = false, Message = "Scheme not found" });

            return Ok(new { Success = true, Data = result });
        }

    }
}
