using APBD_Task_6.Models;
using System.Data.SqlClient;

namespace Zadanie5.Services
{
    public class WarehouseService : IWarehouseService
    {
        private readonly IConfiguration _configuration;

        public WarehouseService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async APBD_Task_6<int> AddProduct(ProductWarehouse productWarehouse)
        {
            var connectionString = _configuration.GetConnectionString("Database");
            using var connection = new SqlConnection(connectionString);
            using var cmd = new SqlCommand();

            cmd.Connection = connection;

            await connection.OpenAsync();

            cmd.CommandText="SELECT TOP 1 [Order].IdOrder FROM [Order] " + 
            "LEFT JOIN Product_Warehouse ON [Order].IdOrder = Product_Warehouse.IdOrder " + 
            "WHERE [Order].IdProduct = @Id Product " +
            "AND [Order].Amount = @Amount " + 
            "AND Product_Warehouse.IdProductWarehouse IS NULL " + 
            "AND [Order].CreatedAt < @CreatedAt";
            
            cmd.Parameters.AddWithValue("IdProduct", productWarehouse.IdProduct);
            cmd.Parameters.AddWithValue("IdProduct", productWarehouse.Amount);
            cmd.Parameters.AddWithValue("IdProduct", productWarehouse.CreatedAt);

            var reader = await cmd.ExecuteReaderAsync();

            if(!reader.HasRows) throw new Exception();

            await reader.ReadAsync();
            int idOrder = int.Parse(reader[idOrder].ToString());
            await reader.CloseAsync();

            cmd.Parameters.Clear();

            cmd.CommandText = "SELECT Price FROM Product WHERE IdProduct = @IdProduct";
            cmd.Parameters.AddWithValue("IdProduct", productWarehouse.IdProduct);

            reader = await cmd.ExecuteReaderAsync();

            if(!reader.HasRows) throw new Exception();

            await reader.ReadAsync();
            double price = double.Parse(reader["Price"].ToString());
            await reader.CloseAsync();

            cmd.Parameters.Clear();

            cmd.CommandText = "SELECT IdWarehouse FROM Warehouse WHERE IdWareHouse = @IdWarehouse";
            cmd.Parameters.AddWithValue("IdWarehouse", productWarehouse.IdWarehouse);

            reader = await cmd.ExecuteReaderAsync();

            if(!reader.HasRows) throw new Exception();

            await reader.ReadAsync();
            int IdWareHouse = int.Parse(reader["IdWarehouse"].ToString());
            await reader.CloseAsync();

            cmd.Parameters.Clear();

            var transaction = (SqlTransaction)await connection.BeginTransactionAsync();
            cmd.Transaction = transaction;

            try{
                cmd.CommandText = "UPDATE [Order] SET FulfilledAt = @CreatedAt WHERE IdOrder = @IdOrder";
                cmd.Parameters.AddWithValue("CreatedAt", productWarehouse.CreatedAt);
                cmd.Parameters.AddWithValue("IdOrder", idOrder);

                int rowsUpdated = await cmd.ExecuteReaderAsync();

                if(rowsUpdated < 1) throw new Exception();

                cmd.Parameters.Clear();

                cmd.CommandText = "INSERT INTO Product_Warehouse(IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)" +
                " VALUES (@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Amount*{price}, @CreatedAt)";

                cmd.Parameters.AddWithValue("IdWarehouse", productWarehouse.IdWarehouse);
                cmd.Parameters.AddWithValue("IdProduct", productWarehouse.IdProduct);
                cmd.Parameters.AddWithValue("IdOrder", idOrder);
                cmd.Parameters.AddWithValue("Amount", productWarehouse.Amount);
                cmd.Parameters.AddWithValue("CreatedAt", productWarehouse.CreatedAt);

                int rowsUpdated = await cmd.ExecuteReaderAsync();

                if(rowsUpdated < 1) throw new Exception();

                await transaction.CommitAsync();


            }catch(Eception){
                await transaction.RollbackAsync();
                throw new Exception();
            }

            cmd.Parameters.Clear();

            cmd.CommandText = "SELECT TOP 1 IdProductWarehouse FROM Product_Warehouse ORDEr BY IdProductWarehouse DESC";

            reader = await cmd.ExecuteReaderAsync();

            await reader.ReadAsync();

            int idProductWarehouse = int.Parse(reader["IdProductWarehouse"].ToString());

            await reader.CloseAsync();

            await connection.CloseAsync();


            return idProductWarehouse;
        }
    }
}
