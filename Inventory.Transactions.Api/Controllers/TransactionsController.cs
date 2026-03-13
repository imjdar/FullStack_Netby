using Microsoft.AspNetCore.Mvc;
using Inventory.Transactions.Api.Application.DTOs;
using Inventory.Transactions.Api.Application.Services;

namespace Inventory.Transactions.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly ILogger<TransactionsController> _logger;

        public TransactionsController(ITransactionService transactionService, ILogger<TransactionsController> logger)
        {
            _transactionService = transactionService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetTransactions(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int? type)
        {
            var transactions = await _transactionService.GetTransactionsAsync(startDate, endDate, type);
            return Ok(transactions);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTransaction([FromBody] TransactionCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _transactionService.CreateTransactionAsync(dto);
            return CreatedAtAction(nameof(GetTransactions), new { id = result?.Id }, result);
        }
    }
}
