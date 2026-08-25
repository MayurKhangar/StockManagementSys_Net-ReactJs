using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SmartStock.Application.Common;
using SmartStock.Application.DTOs.Invoice;
using SmartStock.Application.Interfaces;
using SmartStock.Domain.Entities;

namespace SmartStock.Application.Services;

public class InvoiceService : IInvoiceService
{
    private readonly IUnitOfWork _unitOfWork;

    public InvoiceService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<ResultModel<InvoiceDto>> GetByIdAsync(int id)
    {
        var invoice = await LoadInvoiceAsync(i => i.Id == id);
        return invoice == null
            ? ResultModel<InvoiceDto>.Fail("Invoice not found.")
            : ResultModel<InvoiceDto>.Ok(MapToDto(invoice));
    }

    public async Task<ResultModel<InvoiceDto>> GetByOrderIdAsync(int orderId)
    {
        var invoice = await LoadInvoiceAsync(i => i.OrderId == orderId);
        return invoice == null
            ? ResultModel<InvoiceDto>.Fail("Invoice not found for this order.")
            : ResultModel<InvoiceDto>.Ok(MapToDto(invoice));
    }

    public async Task<ResultModel<List<InvoiceDto>>> GetAllAsync()
    {
        var invoices = await _unitOfWork.Invoices.Query()
            .Include(i => i.Order).ThenInclude(o => o.Customer)
            .Include(i => i.InvoiceItems)
            .OrderByDescending(i => i.IssueDate)
            .ToListAsync();

        return ResultModel<List<InvoiceDto>>.Ok(invoices.Select(MapToDto).ToList());
    }

    public async Task<ResultModel<byte[]>> GeneratePdfAsync(int invoiceId)
    {
        var invoice = await LoadInvoiceAsync(i => i.Id == invoiceId);
        if (invoice == null)
        {
            return ResultModel<byte[]>.Fail("Invoice not found.");
        }

        var dto = MapToDto(invoice);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Column(col =>
                {
                    col.Item().Text("SmartStock").FontSize(22).Bold();
                    col.Item().Text("Store Management System").FontSize(10).FontColor(Colors.Grey.Darken1);
                    col.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                });

                page.Content().PaddingVertical(15).Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Bill To").Bold();
                            c.Item().Text(dto.CustomerName);
                            c.Item().Text(dto.CustomerEmail);
                        });
                        row.RelativeItem().AlignRight().Column(c =>
                        {
                            c.Item().Text($"Invoice #: {dto.InvoiceNumber}").Bold();
                            c.Item().Text($"Order #: {dto.OrderNumber}");
                            c.Item().Text($"Date: {dto.IssueDate:yyyy-MM-dd}");
                        });
                    });

                    col.Item().PaddingTop(20).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(4);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("Product").Bold();
                            header.Cell().AlignRight().Text("Unit Price").Bold();
                            header.Cell().AlignRight().Text("Qty").Bold();
                            header.Cell().AlignRight().Text("Total").Bold();
                            header.Cell().ColumnSpan(4).PaddingTop(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten1);
                        });

                        foreach (var item in dto.Items)
                        {
                            table.Cell().PaddingVertical(4).Text(item.ProductName);
                            table.Cell().PaddingVertical(4).AlignRight().Text($"₹{item.UnitPrice:0.00}");
                            table.Cell().PaddingVertical(4).AlignRight().Text(item.Quantity.ToString());
                            table.Cell().PaddingVertical(4).AlignRight().Text($"₹{item.LineTotal:0.00}");
                        }
                    });

                    col.Item().PaddingTop(15).AlignRight().Column(c =>
                    {
                        c.Item().Text($"Subtotal: ₹{dto.SubTotal:0.00}");
                        c.Item().Text($"Discount: -₹{dto.DiscountAmount:0.00}");
                        c.Item().Text($"Tax: ₹{dto.TaxAmount:0.00}");
                        c.Item().PaddingTop(5).Text($"Total: ₹{dto.TotalAmount:0.00}").Bold().FontSize(14);
                    });
                });

                page.Footer().AlignCenter().Text("Thank you for shopping with SmartStock!").FontSize(9).FontColor(Colors.Grey.Darken1);
            });
        });

        return ResultModel<byte[]>.Ok(document.GeneratePdf());
    }

    private async Task<Invoice?> LoadInvoiceAsync(System.Linq.Expressions.Expression<Func<Invoice, bool>> predicate)
    {
        return await _unitOfWork.Invoices.Query()
            .Include(i => i.Order).ThenInclude(o => o.Customer)
            .Include(i => i.InvoiceItems)
            .FirstOrDefaultAsync(predicate);
    }

    private static InvoiceDto MapToDto(Invoice i) => new()
    {
        Id = i.Id,
        InvoiceNumber = i.InvoiceNumber,
        OrderId = i.OrderId,
        OrderNumber = i.Order?.OrderNumber ?? string.Empty,
        CustomerName = i.Order?.Customer?.FullName ?? string.Empty,
        CustomerEmail = i.Order?.Customer?.Email ?? string.Empty,
        IssueDate = i.IssueDate,
        SubTotal = i.SubTotal,
        DiscountAmount = i.DiscountAmount,
        TaxAmount = i.TaxAmount,
        TotalAmount = i.TotalAmount,
        Items = i.InvoiceItems.Select(ii => new InvoiceItemDto
        {
            ProductName = ii.ProductNameSnapshot,
            UnitPrice = ii.UnitPrice,
            Quantity = ii.Quantity,
            LineTotal = ii.LineTotal
        }).ToList()
    };
}
