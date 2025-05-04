//using BusinessLogicLayer.Interfaces;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Logging;
//using ModelLayer.Models;
//using System;
//using System.Threading.Tasks;

//namespace Insurance.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    [Authorize(Roles = "Employee,Agent")]
//    public class PremiumController : ControllerBase
//    {
//        private readonly IPremiumBL _premiumBL;
//        private readonly ILogger<PremiumController> _logger;

//        public PremiumController(IPremiumBL premiumBL, ILogger<PremiumController> logger)
//        {
//            _premiumBL = premiumBL;
//            _logger = logger;
//        }

//        [HttpPost("calculate")]
//        public async Task<IActionResult> Calculate([FromBody] PremiumModel model)
//        {
//            try
//            {
//                var premium = await _premiumBL.CalculatePremiumAsync(model);
//                return Ok(new
//                {
//                    Success = true,
//                    PremiumAmount = premium
//                });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex,
//                    "Premium calculation failed for Customer {CustomerId}, Plan {PlanId}, Scheme {SchemeId}",
//                    model.CustomerId, model.PlanId, model.SchemeId);
//                return StatusCode(500, new
//                {
//                    Success = false,
//                    Message = ex.Message
//                });
//            }
//        }
//    }
//}
