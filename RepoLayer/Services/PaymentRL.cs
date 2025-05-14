using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ModelLayer.Models;
using ModelLayer.ViewModels;
using RepoLayer.Context;
using RepoLayer.Entity;
using RepoLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using iText.Layout;
using iText.Kernel.Font;
using iText.IO.Font.Constants;



namespace RepoLayer.Services
{
    public class PaymentRL : IPaymentRL
    {
        private readonly ApplicationDbContext _context;

        public PaymentRL(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Payment> ProcessPaymentAsync(Payment payment)
        {
            try
            {
                bool alreadyPaid = await _context.Payments
                    .AnyAsync(p => p.CustomerId == payment.CustomerId && p.PolicyId == payment.PolicyId);

                if (alreadyPaid)
                {
                    throw new Exception("No payment dues. Payment already made for this policy.");
                }

                var paymentIdParam = new SqlParameter("@PaymentId", SqlDbType.Int) { Direction = ParameterDirection.Output };

                var parameters = new[]
                {
                    new SqlParameter("@CustomerId", SqlDbType.Int) { Value = payment.CustomerId },
                    new SqlParameter("@PolicyId", SqlDbType.Int) { Value = payment.PolicyId },
                    new SqlParameter("@Amount", SqlDbType.Decimal) { Value = payment.Amount },
                    new SqlParameter("@PaymentDate", SqlDbType.DateTime) { Value = payment.PaymentDate },
                    new SqlParameter("@CreatedAt", SqlDbType.DateTime) { Value = payment.CreatedAt },
                    paymentIdParam
                };

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC dbo.sp_ProcessPayment @CustomerId, @PolicyId, @Amount, @PaymentDate, @CreatedAt, @PaymentId OUTPUT",
                    parameters);

                var paymentId = Convert.ToInt32(paymentIdParam.Value);
                var createdPayment = await _context.Payments.FindAsync(paymentId);

                return createdPayment ?? throw new Exception("Failed to retrieve newly created payment.");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<PaymentViewModel>> GetCustomerPaymentHistoryAsync(long customerId)
        {
            return await (from payment in _context.Payments
                          join policy in _context.Policies on payment.PolicyId equals policy.PolicyId
                          where payment.CustomerId == customerId
                          select new PaymentViewModel
                          {
                              PaymentId = payment.PaymentId,
                              PolicyId = payment.PolicyId,
                              Amount = payment.Amount,
                              PaymentDate = payment.PaymentDate,
                              PremiumAmount = policy.PremiumAmount,
                              PolicyStatus = policy.Status
                          }).ToListAsync();
        }
        public async Task<ReceiptViewModel> GenerateReceiptAsync(long paymentId)
        {
            try
            {
                ReceiptViewModel receipt = null;

                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = "sp_GetReceiptDetails";  
                    command.CommandType = CommandType.StoredProcedure;

                    var paymentParam = new SqlParameter("@PaymentId", SqlDbType.BigInt)
                    {
                        Value = paymentId
                    };
                    command.Parameters.Add(paymentParam);

                    if (command.Connection.State != ConnectionState.Open)
                        await command.Connection.OpenAsync();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            receipt = new ReceiptViewModel
                            {
                                PaymentId = reader.GetInt32(reader.GetOrdinal("PaymentId")),
                                CustomerName = reader.GetString(reader.GetOrdinal("CustomerName")),
                                Email = reader.GetString(reader.GetOrdinal("Email")),
                                PremiumAmount = reader.GetDecimal(reader.GetOrdinal("PremiumAmount")),
                                Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                                PaymentDate = reader.GetDateTime(reader.GetOrdinal("PaymentDate")),
                                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                            };
                        }
                    }

                    await command.Connection.CloseAsync();
                }

                if (receipt == null)
                {
                    throw new Exception("Receipt not found for the given PaymentId.");
                }

                byte[] fileContent = GeneratePdfReceipt(receipt);

                return new ReceiptViewModel
                {
                    FileContent = fileContent,
                    FileName = $"Receipt_{paymentId}.pdf"
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating receipt for PaymentId: {paymentId}", ex);
            }
        }

        private byte[] GeneratePdfReceipt(ReceiptViewModel receipt)
        {
            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    using (var writer = new PdfWriter(memoryStream))
                    {
                        using (var pdf = new PdfDocument(writer))
                        {
                            var document = new Document(pdf);

                            // Ensure the font is available
                            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

                            // Adding content to the document
                            document.Add(new Paragraph("Receipt")
                                .SetFont(boldFont)
                                .SetFontSize(20));

                            document.Add(new Paragraph($"Payment ID: {receipt.PaymentId}"));
                            document.Add(new Paragraph($"Customer Name: {receipt.CustomerName}"));
                            document.Add(new Paragraph($"Email: {receipt.Email}"));
                            document.Add(new Paragraph($"Premium Amount: {receipt.PremiumAmount:C}"));
                            document.Add(new Paragraph($"Amount Paid: {receipt.Amount:C}"));
                            document.Add(new Paragraph($"Payment Date: {receipt.PaymentDate:yyyy-MM-dd}"));
                            document.Add(new Paragraph($"Created At: {receipt.CreatedAt:yyyy-MM-dd HH:mm:ss}"));

                            document.Close();  // Properly close the document
                        }
                    }

                    return memoryStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                // Log or throw the exception as needed for debugging
                throw new Exception($"Error generating PDF for PaymentId: {receipt.PaymentId}", ex);
            }
        }

    }
}
