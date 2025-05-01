using BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModelLayer.Models;
using RepoLayer.Entity;
using System;
using System.Threading.Tasks;

namespace Insurance.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Employee,Agent")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentBL _paymentBL;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IPaymentBL paymentBL, ILogger<PaymentController> logger)
        {
            _paymentBL = paymentBL;
            _logger = logger;
        }

        [HttpPost("process")]
        public async Task<IActionResult> ProcessPayment([FromBody] PaymentModel model)
        {
            try
            {
                var result = await _paymentBL.ProcessPaymentAsync(model);
                return Ok(new ResponseModel<Payment>
                {
                    Success = true,
                    Message = "Payment processed successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payment processing failed for PolicyId {PolicyId}", model.PolicyId);
                return StatusCode(500, new ResponseModel<string>
                {
                    Success = false,
                    Message = "Payment processing failed",
                    Data = ex.Message
                });
            }
        }

        [HttpGet("history/{customerId}")]
        [Authorize(Roles = "Admin,Employee,Agent,Customer")]
        public async Task<IActionResult> GetPaymentHistory(int customerId)
        {
            var result = await _paymentBL.GetCustomerPaymentHistoryAsync(customerId);
            return Ok(new
            {
                Success = true,
                Message = "Payment history fetched successfully",
                Data = result
            });
        }

    }
}
