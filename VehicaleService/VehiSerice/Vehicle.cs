
namespace VehicleTest
{
public class Vehicle
{
    // Properties
    public int ID { get; set; }
    public string Brand { get; set; }
    public string Model { get; set; }
    public string RegistrationNumber { get; set; }
    public int Mileage { get; set; }
    public List<ServiceHistory>? ServiceHistory { get; set; }
    public List<Image>? ImageHistory { get; set; }

    // Constructor
    public Vehicle(int id, string brand, string model, string registrationNumber, int mileage)
    {
        ID = id;
        Brand = brand;
        Model = model;
        RegistrationNumber = registrationNumber;
        Mileage = mileage;
        ServiceHistory = new List<ServiceHistory>();
        ImageHistory = new List<Image>();
    }

}
}





