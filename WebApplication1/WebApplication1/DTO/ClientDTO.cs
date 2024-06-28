namespace WebApplication1.DTO;

public class ClientDTO
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Address { get; set; }
    public List<RentalsDTO> Rentals { get; set; }
}

public class RentalsDTO
{
    public string VIN { get; set; }
    public string Color { get; set; }
    public string Model { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public decimal TotalPrice { get; set; }
}