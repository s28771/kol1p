using WebApplication1.DTO;

public interface ICarRepo
{
    Task<bool> DoesClientExists(int id);
    Task<ClientDTO> GetClientData(int id);
    Task<bool> DoesCarExists(int id);
    Task<decimal> GetPriceCarPerDay(int id);
    Task<int> AddClient(string firstName, string lastName, string address);
    Task<int> AddCar(int carId, DateTime dateFrom, DateTime dateTo);
    Task AddClientAndCar(int clientId, int carId, DateTime dateFrom, DateTime dateTo); 
}