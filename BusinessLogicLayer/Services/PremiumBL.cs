//using BusinessLogicLayer.Interfaces;
//using Microsoft.Extensions.Logging;
//using ModelLayer.Models;
//using RepoLayer.Interfaces;
//using System;
//using System.Threading.Tasks;

//namespace BusinessLogicLayer.Services
//{
//    public class PremiumBL : IPremiumBL
//    {
//        private readonly IPremiumRL _premiumRL;
//        private readonly ILogger<PremiumBL> _logger;

//        public PremiumBL(IPremiumRL premiumRL, ILogger<PremiumBL> logger)
//        {
//            _premiumRL = premiumRL;
//            _logger = logger;
//        }

//        public async Task<decimal> CalculatePremiumAsync(PremiumModel model)
//        {
//            try
//            {
//                var premium = await _premiumRL.CalculatePremiumAsync(model);
//                _logger.LogInformation(
//                    "Calculated premium {Premium:C} for Customer {CustomerId}, Plan {PlanId}, Scheme {SchemeId}, Period {Years}yr",
//                    premium, model.CustomerId, model.PlanId, model.SchemeId, model.MaturityPeriod);
//                return premium;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex,
//                    "Error calculating premium for Customer {CustomerId}, Plan {PlanId}, Scheme {SchemeId}",
//                    model.CustomerId, model.PlanId, model.SchemeId);
//                throw;
//            }
//        }
//    }
//}
