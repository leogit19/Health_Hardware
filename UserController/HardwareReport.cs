using System.Collections.Generic;
namespace UserController
{
    public class HardwareReport
    {
        public string MachineName { get; set; } = System.Environment.MachineName;
        public System.DateTime ScanDate { get; set; } = System.DateTime.UtcNow;
        public BatteryInfo Battery { get; set; } = new();
        public RamInfo Ram { get; set; } = new();
        public List<DiskInfo> Disks { get; set; } = new();
        public CmosInfo Cmos { get; set; } = new();

        public CpuInfo Cpu { get; set; } = new();
        public List<GpuInfo> Gpus { get; set; } = new();
        public MotherboardInfo Motherboard { get; set; } = new();
        public List<FanInfo> Fans { get; set; } = new();
        public NetworkModuleInfo Network { get; set; } = new();
    }
    public class CpuInfo
    {
        public string Name { get; set; } = string.Empty;
        public uint LoadPercentage { get; set; }
        public uint Cores { get; set; }
        public uint LogicalProcessors { get; set; }
        public uint MaxClockSpeedMhz { get; set; }
        public double L3CacheMb { get; set;}

        public double SinglCoreScore { get; set; }
        public double MultiCoreScore { get; set; }
        public string PerformanceTier { get; set; } = "Necunoscut";
        public string Status { get; set; } = "OK";
        public bool IsWorkingOk { get; set; } = true;
    }
    public class GpuInfo
    {
        public string Name { get; set; } = string.Empty;
        public string DriverVersion { get; set; } = string.Empty;
        public string Status { get; set; } = "OK";
        public bool IsWorkingOk { get;set;  } = true;
    }
    public class MotherboardInfo
    {
        public string Manufacturer {  get; set; } = string.Empty;
        public string Product { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string Status { get; set; } = "OK";
        public bool IsWorkingOk { get; set; } = true;
    }
    public class FanInfo
    {
        public string Name { get; set; } = "Ventilator";
        public string Status { get; set; } = "OK";
        public bool IsWorkingOk { get;set;  } = true;
    }
    public class WirelessDevice
    {
        public bool Found { get; set; }
        public string Name { get; set; } = "Nedeclarat";
        public string Technology { get; set; } = "Necunoscuta";
        public string Status { get; set; } = "Necunoscut";
        public bool IsWorkingOk { get; set; }
    }
    public class BatteryInfo
    {
        public bool Present { get; set; }
        public string Name { get; set; } = string.Empty;
        public uint ChargePercentage { get; set; }
        public uint DesignCapacityMWh { get; set; }
        public uint FullChargeCapacityMwh { get; set; }
        public double HealthPercentage { get; set; }
        public double WearPercentage { get; set; }
        public string Status { get; set; } = string.Empty;
    }
    public class RamInfo
    {
        public double TotalGb { get; set;  }
        public double FreeGb { get; set; }
        public double UsedGb { get; set; }
        public double UsagePercentage { get; set; }

        public bool isHealthy { get; set; } = true;
        public string HealthStatus {  get; set; } = "Sanatos ,0 erori";
        public int TotalModulesFound { get; set; }
        public List<RamModuleDetail> Modules { get; set; } = new();
    }
    public class RamModuleDetail
    {
        public string BankLabel { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public double CapacityGb { get; set; }
        public uint SpeedMhz { get; set; }
        public string Status { get; set; } = "OK";
        public bool IsWorking { get; set; } = true;
    }

    public class DiskInfo
    {
        public string Model { get; set; } = string.Empty;
        public double SizeGb { get; set; }
        public string InterfaceType { get; set;  } = "SATA/NVMe";
        public bool IsHealthy { get; set; } = true;
        public double HealthPercentage { get; set; } = 100.0;
        public bool PredictFailure { get; set;  } = false;
        
        public string SmartStatus { get; set; } = "OK";

        public string HealthRecommendation { get; set; } = "Discul ruleaza fara probleme";
    }

    public class CmosInfo
    {
        public string Status { get; set; } = "Necunoscut";
        public string BatteryStatus { get; set; } = "Necunoscut";
        public string BiosManufacturer { get; set; } = string.Empty;
        public string Biosversion { get; set; } = string.Empty;
    }
    public class NetworkModuleInfo
    {
        public WirelessDevice Wifi { get; set; } = new();
        public WirelessDevice Bluetooth { get; set; } = new();
    }
}
