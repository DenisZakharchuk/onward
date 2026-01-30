using InventorySystem.Business.Services;
using InventorySystem.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace InventorySystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StockController : ControllerBase
{
    private readonly StockService _stockService;
    private readonly ILogger<StockController> _logger;

    public StockController(StockService stockService, ILogger<StockController> logger)
    {
        _stockService = stockService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<StockMovementDto>>> GetAll(CancellationToken cancellationToken)
    {
        var movements = await _stockService.GetAllMovementsAsync(cancellationToken);
        return Ok(movements);
    }

    [HttpGet("product/{productId}")]
    public async Task<ActionResult<IEnumerable<StockMovementDto>>> GetByProduct(Guid productId, CancellationToken cancellationToken)
    {
        var movements = await _stockService.GetProductMovementsAsync(productId, cancellationToken);
        return Ok(movements);
    }

    [HttpPost]
    public async Task<ActionResult<StockMovementDto>> Create([FromBody] CreateStockMovementDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var movement = await _stockService.CreateStockMovementAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetByProduct), new { productId = movement.ProductId }, movement);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating stock movement");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }
}
