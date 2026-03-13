using Inventory.Transactions.Api.Application.DTOs;

namespace Inventory.Transactions.Api.Application.Services
{
    public interface ITransactionService
    {
        Task<IEnumerable<TransactionResponseDto>> GetTransactionsAsync(DateTime? startDate, DateTime? endDate, int? type);
        Task<TransactionResponseDto?> CreateTransactionAsync(TransactionCreateDto dto);
    }
}
