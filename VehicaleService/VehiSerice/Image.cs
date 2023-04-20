namespace VehicleTest
{
public class Image
{
    // Properties
    public int ID { get; set; }
    public string Location { get; set; }
    public DateTime Timestamp { get; set; }
    public string Description { get; set; }
    public string AddedBy { get; set; }

    // Constructor
    public Image(int id, string location, DateTime timestamp, string description, string addedBy)
    {
        ID = id;
        Location = location;
        Timestamp = timestamp;
        Description = description;
        AddedBy = addedBy;
    }
}
}