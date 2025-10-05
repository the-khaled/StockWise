using StockWise.Domain.Interfaces;
using StockWise.Domain.Models;
using StockWise.Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.Services
{
    public class TransferService : ITransferService
    {
        private IUnitOfWork _unitOfWork;
        public TransferService( IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public Task CreateTransferAsync(Transfer transfer)
        {
            throw new NotImplementedException();
        }

        public Task DeleteTransferAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Transfer>> GetAllTransfersAsync()
        {
            return null; ////////////////////////////////
        }

        public Task<Transfer> GetTransferByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateTransferAsync(Transfer transfer)
        {
            throw new NotImplementedException();
        }
    }
}
