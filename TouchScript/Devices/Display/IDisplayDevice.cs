namespace TouchScript.Devices.Display
{
    public interface IDisplayDevice
    {
        string Name { get; set; }

        float DPI { get; set; }
    }
}
