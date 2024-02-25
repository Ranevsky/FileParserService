namespace FileParserService.Models.Generated;

public class CombinedPumpStatus
{
    public string ModuleState { get; set; }
    public bool IsBusy { get; set; }
    public bool IsReady { get; set; }
    public bool IsError { get; set; }
    public bool KeyLock { get; set; }
    public string Mode { get; set; }
    public int Flow { get; set; }
    public int PercentB { get; set; }
    public int PercentC { get; set; }
    public int PercentD { get; set; }
    public int MinimumPressureLimit { get; set; }
    public double MaximumPressureLimit { get; set; }
    public int Pressure { get; set; }
    public bool PumpOn { get; set; }
    public int Channel { get; set; }
    public string xsi { get; set; }
    public string xsd { get; set; }
}