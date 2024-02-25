namespace FileParserService.Models.Generated;

public class InstrumentStatus
{
    public string PackageID { get; set; }
    public ICollection<DeviceStatus> DeviceStatus { get; set; }
    public string xsi { get; set; }
    public string xsd { get; set; }
    public string schemaVersion { get; set; }
}