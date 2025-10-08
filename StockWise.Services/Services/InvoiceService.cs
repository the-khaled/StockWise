using StockWise.Domain.Interfaces;
using StockWise.Domain.Models;
using StockWise.Services.DTOS;
using StockWise.Services.Exceptions;
using StockWise.Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IUnitOfWork _unitOfWork;
        public InvoiceService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task CreatInvoiceAsync(InvoiceDto invoiceDto)
        {
            //1
            if (invoiceDto == null) throw new ArgumentNullException(nameof(invoiceDto)); 
            //2 TotalAmount
            if (invoiceDto.TotalAmount.Amount < 0)
                throw new BusinessException("Total amount cannot be negative.");
            //3 customer
            var customer = await _unitOfWork.Customer.GetByIdAsync(invoiceDto.CustomerId);
            if (customer == null)
                throw new BusinessException("Customer not found.");
            //4 representative
            var representative = await _unitOfWork.Representatives.GetByIdAsync(invoiceDto.RepresentativeId);
            if (representative == null)
                throw new BusinessException("Representative not found.");

            await _unitOfWork.Invoice.AddAsync(MapToEntity(invoiceDto));
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteInvoiceAsync(int id)
        {
           var invoice =await _unitOfWork.Invoice.GetByIdAsync(id);
            if(invoice == null)throw new KeyNotFoundException($"Invoise with ID {id} Not Found .");
            await _unitOfWork.Invoice.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<InvoiceDto>> GetAllInvoicesAsync()
        {
            var invoice = await _unitOfWork.Invoice.GetAllAsync();
            return invoice.Select(i=>MapToDto(i)).ToList();
        }

        public async Task<InvoiceDto> GetInvoiceByIdAsync(int id)
        {
           var invoice = await _unitOfWork.Invoice.GetByIdAsync(id);
            if (invoice == null) throw new KeyNotFoundException($"Invoice with ID {id} not found.");
            return MapToDto(invoice);
        }

        public async Task UpdateInvoiceAsync(InvoiceDto invoiceDto)
        {
            if (invoiceDto == null) 
                throw new ArgumentNullException(nameof(invoiceDto));

            var ExisteInvoice = await _unitOfWork.Invoice.GetByIdAsync(invoiceDto.Id);
            if(ExisteInvoice == null) 
                throw new KeyNotFoundException($"Invoice with ID {invoiceDto.Id} not found.");

            var customer = await _unitOfWork.Customer.GetByIdAsync(invoiceDto.CustomerId);
            if (customer == null)
                throw new BusinessException("Customer not found.");

            var representative = await _unitOfWork.Representatives.GetByIdAsync(invoiceDto.RepresentativeId);
            if (representative == null)
                throw new BusinessException("Representative not found.");

            ExisteInvoice.CustomerId = invoiceDto.CustomerId;
            ExisteInvoice.RepresentativeId = invoiceDto.RepresentativeId;
            ExisteInvoice.TotalAmount = invoiceDto.TotalAmount;
            ExisteInvoice.UpdatedAt = DateTime.Now;
            await _unitOfWork.Invoice.UpdateAsync(ExisteInvoice);
            await _unitOfWork.SaveChangesAsync();
        }
        public InvoiceDto MapToDto(Invoice invoice) 
        {
            return new InvoiceDto
            {
                Id = invoice.Id,
                CustomerId = invoice.CustomerId,
                RepresentativeId = invoice.RepresentativeId,
                TotalAmount = invoice.TotalAmount,
                InvoiceItems = invoice.Items?.Select(i => new InvoiceItemDto 
                {
                    Id = i.Id,
                    InvoiceId = i.InvoiceId,
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Price = i.Price,
                    CreatedAt = i.CreatedAt,
                    UpdatedAt =i.UpdatedAt
                }).ToList(),
                CreatedAt = invoice.CreatedAt,
                UpdatedAt = invoice.UpdatedAt
            };
        }
        public Invoice MapToEntity(InvoiceDto invoicedto)
        {
            return new Invoice
            {
                Id = invoicedto.Id,
                CustomerId = invoicedto.CustomerId,
                RepresentativeId = invoicedto.RepresentativeId,
                TotalAmount = invoicedto.TotalAmount,
                Items = invoicedto.InvoiceItems?.Select(i => new InvoiceItem
                {
                    Id = i.Id,
                    InvoiceId = i.InvoiceId,
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Price = i.Price,
                    CreatedAt = i.CreatedAt,
                    UpdatedAt = i.UpdatedAt
                }).ToList(),
                CreatedAt = invoicedto.CreatedAt,
                UpdatedAt = invoicedto.UpdatedAt
            };
        }
    }
}
/*
  public int Id { get; set; }

        [Required(ErrorMessage = "CustomerId is required.")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "RepresentativeId is required.")]
        public int RepresentativeId { get; set; }

        [Required(ErrorMessage = "TotalAmount is required.")]
        public Money TotalAmount { get; set; }

        public List<InvoiceItemDto> InvoiceItems { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
 */