using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class WarehouseController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public WarehouseController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost]
    public async Task<IActionResult> AddProductToWarehouse([FromBody] WarehouseRequest request)
    {
        if (request.Amount <= 0)
            return BadRequest("Amount must be greater than 0.");

        using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await connection.OpenAsync();

        var productExists = await CheckExistsAsync(connection, "SELECT COUNT(*) FROM Product WHERE IdProduct = @IdProduct", "@IdProduct", request.IdProduct);
        if (!productExists)
            return NotFound("Product not found.");

        var warehouseExists = await CheckExistsAsync(connection, "SELECT COUNT(*) FROM Warehouse WHERE IdWarehouse = @IdWarehouse", "@IdWarehouse", request.IdWarehouse);
        if (!warehouseExists)
            return NotFound("Warehouse not found.");

        var orderId = await GetValidOrderIdAsync(connection, request);
        if (orderId == null)
            return BadRequest("No valid order found for the given product and amount.");

        var orderCompleted = await CheckExistsAsync(connection, "SELECT COUNT(*) FROM Product_Warehouse WHERE IdOrder = @IdOrder", "@IdOrder", orderId.Value);
        if (orderCompleted)
            return BadRequest("The order has already been completed.");

        await UpdateOrderFulfilledAtAsync(connection, orderId.Value);

        var productWarehouseId = await InsertIntoProductWarehouseAsync(connection, request, orderId.Value);


        return Ok(new { IdProductWarehouse = productWarehouseId });
    }

    private async Task<bool> CheckExistsAsync(SqlConnection connection, string query, string parameterName, int parameterValue)
    {
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue(parameterName, parameterValue);
        var count = (int)await command.ExecuteScalarAsync();
        return count > 0;
    }


    private async Task<int?> GetValidOrderIdAsync(SqlConnection connection, WarehouseRequest request)
    {
        const string query = @"
            SELECT IdOrder 
            FROM [Order] 
            WHERE IdProduct = @IdProduct 
              AND Amount = @Amount 
              AND CreatedAt < @CreatedAt";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@IdProduct", request.IdProduct);
        command.Parameters.AddWithValue("@Amount", request.Amount);
        command.Parameters.AddWithValue("@CreatedAt", request.CreatedAt);

        var result = await command.ExecuteScalarAsync();
        return result == null ? (int?)null : Convert.ToInt32(result);
    }

    private async Task UpdateOrderFulfilledAtAsync(SqlConnection connection, int orderId)
    {
        const string query = "UPDATE [Order] SET FulfilledAt = @FulfilledAt WHERE IdOrder = @IdOrder";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@FulfilledAt", DateTime.UtcNow);
        command.Parameters.AddWithValue("@IdOrder", orderId);

        await command.ExecuteNonQueryAsync();
    }

    private async Task<int> InsertIntoProductWarehouseAsync(SqlConnection connection, WarehouseRequest request, int orderId)
    {
        const string query = @"
            INSERT INTO Product_Warehouse (IdProduct, IdWarehouse, IdOrder, Amount, Price, CreatedAt)
            OUTPUT INSERTED.IdProductWarehouse
            VALUES (@IdProduct, @IdWarehouse, @IdOrder, @Amount, @Price, @CreatedAt)";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@IdProduct", request.IdProduct);
        command.Parameters.AddWithValue("@IdWarehouse", request.IdWarehouse);
        command.Parameters.AddWithValue("@IdOrder", orderId);
        command.Parameters.AddWithValue("@Amount", request.Amount);

        var price = await GetProductPriceAsync(connection, request.IdProduct) * request.Amount;
        command.Parameters.AddWithValue("@Price", price);
        command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);

        return (int)await command.ExecuteScalarAsync();
    }

    private async Task<decimal> GetProductPriceAsync(SqlConnection connection, int productId)
    {
        const string query = "SELECT Price FROM Product WHERE IdProduct = @IdProduct";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@IdProduct", productId);
        var price = await command.ExecuteScalarAsync();
        return Convert.ToDecimal(price);
    }


    [HttpPost("add-using-procedure")]
    public async Task<IActionResult> AddProductToWarehouseUsingProcedure([FromBody] WarehouseRequest request)
    {
        if (request.Amount <= 0)
            return BadRequest("Amount must be greater than 0.");

        using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
        using (var command = new SqlCommand("AddProductToWarehouse", connection))
        {
            command.CommandType = System.Data.CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@IdProduct", request.IdProduct);
            command.Parameters.AddWithValue("@IdWarehouse", request.IdWarehouse);
            command.Parameters.AddWithValue("@Amount", request.Amount);
            command.Parameters.AddWithValue("@CreatedAt", request.CreatedAt);

            try
            {
                await connection.OpenAsync();
                var result = await command.ExecuteScalarAsync();

                if (result == null)
                    return StatusCode(500, "An unexpected error occurred.");

                return Ok(new { IdProductWarehouse = Convert.ToInt32(result) });
            }
            catch (SqlException ex)
            {
                if (ex.Message.Contains("Invalid parameter"))
                    return BadRequest(ex.Message);

                return StatusCode(500, $"Database error: {ex.Message}");
            }
        }
    }

}
