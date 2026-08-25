using SmartStock.Application.Common;
using SmartStock.Application.DTOs.Invoice;

namespace SmartStock.Application.Interfaces;

public interface IInvoiceService
{
    Task<ResultModel<InvoiceDto>> GetByIdAsync(int id);
    Task<ResultModel<InvoiceDto>> GetByOrderIdAsync(int orderId);
    Task<ResultModel<List<InvoiceDto>>> GetAllAsync();
    Task<ResultModel<byte[]>> GeneratePdfAsync(int invoiceId);
}
