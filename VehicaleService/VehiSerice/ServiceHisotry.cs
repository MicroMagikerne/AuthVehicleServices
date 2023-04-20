namespace VehicleTest
{
public class ServiceHistory
{
    // Properties
    public int ID { get; set; }
    public DateTime Timestamp { get; set; }
    public string Description { get; set; }
    public string ServicedBy { get; set; }

    // Constructor
    public ServiceHistory(int id, DateTime timestamp, string description, string servicedBy)
    {
        ID = id;
        Timestamp = timestamp;
        Description = description;
        ServicedBy = servicedBy;
    }
}
}