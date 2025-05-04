using BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;
using ModelLayer.Models;
using System.Threading.Tasks;

namespace Insurance.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SchemeController : ControllerBase
    {
        private readonly ISchemeBL _schemeBL;

        public SchemeController(ISchemeBL schemeBL)
        {
            _schemeBL = schemeBL;
        }

        [HttpPost]
        public async Task<IActionResult> CreateScheme([FromBody] SchemeModel model)
        {
            var result = await _schemeBL.CreateSchemeAsync(model);
            return Ok(new { Success = true, Message = "Scheme created", Data = result });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSchemes()
        {
            var result = await _schemeBL.GetAllSchemesAsync();
            return Ok(new { Success = true, Data = result });
        }
    }
}
