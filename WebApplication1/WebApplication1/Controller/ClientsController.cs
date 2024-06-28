namespace WebApplication1.Controller;


using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class ClientsController : ControllerBase
{
    private readonly ICarRepo _carRepo;

    public ClientsController(ICarRepo carRepo)
    {
        _carRepo = carRepo;
    }

    [HttpGet("{clientId}")]
    public async Task<IActionResult> GetClientWithRentals(int clientId)
    {
        var client = await _carRepo.GetClientData(clientId);
        if (client == null)
        {
            return NotFound();
        }
        return Ok(client);
    }

    [HttpPost]
    public async Task<IActionResult> AddClientWithRental([FromBody] AddClientWithRentalRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        bool carExists = await _carRepo.DoesCarExists(request.CarID);
        if (!carExists)
        {
            return BadRequest("Car does not exist");
        }

        int clientId = await _carRepo.AddClient(request.Client.FirstName, request.Client.LastName, request.Client.Address);
        await _carRepo.AddClientAndCar(clientId, request.CarID, request.DateFrom, request.DateTo);

        return CreatedAtAction(nameof(GetClientWithRentals), new { clientId = clientId }, null);
    }
}

public class AddClientWithRentalRequest
{
    public ClientRequest Client { get; set; }
    public int CarID { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
}

public class ClientRequest
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Address { get; set; }
}
