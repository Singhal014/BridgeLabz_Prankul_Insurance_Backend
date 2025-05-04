using BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;
using ModelLayer.Models;
using System.Threading.Tasks;

namespace Insurance.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PlanController : ControllerBase
    {
        private readonly IPlanBL _planBL;

        public PlanController(IPlanBL planBL)
        {
            _planBL = planBL;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePlan([FromBody] PlanModel model)
        {
            var result = await _planBL.CreatePlanAsync(model);
            return Ok(new { Success = true, Message = "Plan created", Data = result });
        }

        [HttpGet]
        public async Task<IActionResult> GetPlans()
        {
            var result = await _planBL.GetAllPlansAsync();
            return Ok(new { Success = true, Data = result });
        }
    }
}
