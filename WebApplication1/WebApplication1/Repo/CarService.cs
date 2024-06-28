using Microsoft.Data.SqlClient;
using WebApplication1.DTO;

namespace WebApplication11.Repo;

using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

public class CarService : ICarRepo
{
    private readonly IConfiguration _configuration;

    public CarService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<bool> DoesClientExists(int id)
    {
        var query = "SELECT 1 FROM Clients WHERE ID = @ID";
        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        using SqlCommand command = new SqlCommand(query, connection);
        
        command.Parameters.AddWithValue("@ID", id);
        
        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();

        return result != null;
    }

    public async Task<ClientDTO> GetClientData(int id)
    {
        var query = @"
            SELECT 
                Clients.ID AS ClientID,
                Clients.FirstName,
                Clients.LastName,
                Clients.Address,
                Cars.VIN,
                Colors.Name AS Color,
                Models.Name AS Model,
                Car_Rentals.DateFrom,
                Car_Rentals.DateTo,
                Car_Rentals.TotalPrice
            FROM Clients
            INNER JOIN Car_Rentals ON Clients.ID = Car_Rentals.ClientID
            INNER JOIN Cars ON Car_Rentals.CarID = Cars.ID
            INNER JOIN Colors ON Cars.ColorID = Colors.ID
            INNER JOIN Models ON Cars.ModelID = Models.ID
            WHERE Clients.ID = @ID";

        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        using SqlCommand command = new SqlCommand(query, connection);

        command.Parameters.AddWithValue("@ID", id);

        await connection.OpenAsync();

        List<RentalsDTO> rentals = new List<RentalsDTO>();

        using SqlDataReader reader = await command.ExecuteReaderAsync();

        ClientDTO clientDto = null;

        while (await reader.ReadAsync())
        {
            if (clientDto == null)
            {
                clientDto = new ClientDTO
                {
                    Id = reader.GetInt32(reader.GetOrdinal("ClientID")),
                    FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                    LastName = reader.GetString(reader.GetOrdinal("LastName")),
                    Address = reader.GetString(reader.GetOrdinal("Address")),
                    Rentals = new List<RentalsDTO>()
                };
            }

            rentals.Add(new RentalsDTO
            {
                VIN = reader.GetString(reader.GetOrdinal("VIN")),
                Color = reader.GetString(reader.GetOrdinal("Color")),
                Model = reader.GetString(reader.GetOrdinal("Model")),
                DateFrom = reader.GetDateTime(reader.GetOrdinal("DateFrom")),
                DateTo = reader.GetDateTime(reader.GetOrdinal("DateTo")),
                TotalPrice = reader.GetDecimal(reader.GetOrdinal("TotalPrice"))
            });
        }

        if (clientDto == null)
        {
            throw new Exception($"Client with ID {id} not found.");
        }

        clientDto.Rentals = rentals;

        return clientDto;
    }

    public async Task<bool> DoesCarExists(int id)
    {
        var query = "SELECT 1 FROM Cars WHERE ID = @ID";
        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        using SqlCommand command = new SqlCommand(query, connection);

        command.Parameters.AddWithValue("@ID", id);

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();

        return result != null;
    }

    public async Task<decimal> GetPriceCarPerDay(int id)
    {
        var query = "SELECT PricePerDay FROM Cars WHERE ID = @ID";
        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        using SqlCommand command = new SqlCommand(query, connection);

        command.Parameters.AddWithValue("@ID", id);

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();

        return Convert.ToDecimal(result);
    }

    public async Task<int> AddClient(string firstName, string lastName, string address)
    {
        var query = "INSERT INTO Clients (FirstName, LastName, Address) VALUES (@FirstName, @LastName, @Address); SELECT SCOPE_IDENTITY();";

        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        using SqlCommand command = new SqlCommand(query, connection);

        command.Parameters.AddWithValue("@FirstName", firstName);
        command.Parameters.AddWithValue("@LastName", lastName);
        command.Parameters.AddWithValue("@Address", address);

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();

        return Convert.ToInt32(result);
    }

    public async Task<int> AddCar(int carId, DateTime dateFrom, DateTime dateTo)
    {
        var query = "INSERT INTO Car_Rentals (CarID, DateFrom, DateTo, TotalPrice) VALUES (@CarID, @DateFrom, @DateTo, @TotalPrice); SELECT SCOPE_IDENTITY();";

        var totalPrice = await CalculateTotalPrice(carId, dateFrom, dateTo);

        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        using SqlCommand command = new SqlCommand(query, connection);

        command.Parameters.AddWithValue("@CarID", carId);
        command.Parameters.AddWithValue("@DateFrom", dateFrom);
        command.Parameters.AddWithValue("@DateTo", dateTo);
        command.Parameters.AddWithValue("@TotalPrice", totalPrice);

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();

        return Convert.ToInt32(result);
    }

    public async Task AddClientAndCar(int clientId, int carId, DateTime dateFrom, DateTime dateTo)
    {
        var query = "INSERT INTO Car_Rentals (ClientID, CarID, DateFrom, DateTo, TotalPrice) VALUES (@ClientID, @CarID, @DateFrom, @DateTo, @TotalPrice);";

        var totalPrice = await CalculateTotalPrice(carId, dateFrom, dateTo);

        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        using SqlCommand command = new SqlCommand(query, connection);

        command.Parameters.AddWithValue("@ClientID", clientId);
        command.Parameters.AddWithValue("@CarID", carId);
        command.Parameters.AddWithValue("@DateFrom", dateFrom);
        command.Parameters.AddWithValue("@DateTo", dateTo);
        command.Parameters.AddWithValue("@TotalPrice", totalPrice);

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    private async Task<int> CalculateTotalPrice(int carId, DateTime dateFrom, DateTime dateTo)
    {
        var pricePerDay = await GetPriceCarPerDay(carId);
        var totalDays = (int)Math.Ceiling((dateTo - dateFrom).TotalDays);

        return (int)(pricePerDay * totalDays);
    }
}
