using APBD_Task_6.Models;

namespace Zadanie5.Services
{
    public interface IWarehouseService
    {
        Task<int> AddProduct(ProductWarehouse productWarehouse);
    }
}
