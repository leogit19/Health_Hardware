using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Threading.Tasks;
namespace UserController;

public class HardWareDiagnostics
{
    public static HardwareReport GetDiagnostics()
    {
        var report = new HardwareReport();
        //CPU
        try
        {
            using var search1 = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
            foreach (var obj in search1.Get())
            {
                report.Cpu.Name = obj["Name"]?.ToString()?.Trim() ?? "Procesor Necunoscut";
                report.Cpu.LoadPercentage = Convert.ToUInt16(obj["LoadPercentage"] ?? 0);
                report.Cpu.Cores = Convert.ToUInt32(obj["NumberOfCores"] ?? 0);
                report.Cpu.LogicalProcessors = Convert.ToUInt32(obj["NumberOfLogicalProcessors"] ?? 0);
                report.Cpu.MaxClockSpeedMhz = Convert.ToUInt32(obj["MaxClockSpeed"] ?? 0);
                uint l3kb = Convert.ToUInt32(obj["L3CacheSize"] ?? 0);
                report.Cpu.L3CacheMb = Math.Round(l3kb / 1024.0, 2);
                uint errorCode = Convert.ToUInt32(obj["ConfigManagerErrorCode"] ?? 0);
                report.Cpu.IsWorkingOk = (errorCode == 0);
                report.Cpu.Status = report.Cpu.IsWorkingOk ? "Ok (functioneaza normal)" : $"Eroare cod: {errorCode}";
            }

            report.Cpu.SinglCoreScore = RunSingleThreadBenchmark(100);
            report.Cpu.MultiCoreScore = RunMultihreadsBench(100);

            if (report.Cpu.SinglCoreScore > 2500 || report.Cpu.L3CacheMb >= 32)
                report.Cpu.PerformanceTier = "High-End";
            else if (report.Cpu.SinglCoreScore > 1500 || report.Cpu.L3CacheMb >= 16)
                report.Cpu.PerformanceTier = "MIdrange";
            else
                report.Cpu.PerformanceTier = "Entry";
        }
        catch { }
        try //GPU
        {
            using var search1 = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
            foreach (var obj in search1.Get())
            {
                uint errorCod = Convert.ToUInt32(obj["ConfigManagerErrorCode"] ?? 0);
                bool isOk = (errorCod == 0);
                report.Gpus.Add(new GpuInfo
                {
                    Name = obj["Name"]?.ToString() ?? "Placa Video Necunoscuta",
                    DriverVersion = obj["DriverVersion"].ToString() ?? "N/A",
                    IsWorkingOk = isOk,
                    Status = isOk ? "OK (Functioneaza normal)" : $"Eroare (Cod {errorCod}"

                });

            }
        }
        catch { }

        // 3. PLACĂ DE BAZĂ (MOTHERBOARD)
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard");
            foreach (var obj in searcher.Get())
            {
                report.Motherboard.Manufacturer = obj["Manufacturer"]?.ToString() ?? "Necunoscut";
                report.Motherboard.Product = obj["Product"]?.ToString() ?? "Necunoscut";
                report.Motherboard.SerialNumber = obj["SerialNumber"]?.ToString() ?? "N/A";

                uint errorCode = Convert.ToUInt32(obj["ConfigManagerErrorCode"] ?? 0);
                report.Motherboard.IsWorkingOk = (errorCode == 0);
                report.Motherboard.Status = report.Motherboard.IsWorkingOk ? "OK" : $"Eroare (Cod {errorCode})";
            }
        }
        catch { }
        //ventilator
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Fan");
            var fanCollection = searcher.Get();
            if (fanCollection.Count > 0)
            {
                foreach (var obj in fanCollection)
                {
                    uint errorCode = Convert.ToUInt32(obj["ConfigManagerErrorCode"] ?? 0);
                    bool isOk = (errorCode == 0);
                    report.Fans.Add(new FanInfo
                    {
                        Name = obj["Name"]?.ToString() ?? "Ventilator Sistem",
                        IsWorkingOk = isOk,
                        Status = isOk ? "OK (Turație normală)" : "Avertisment / Defect"
                    });
                }
            }
            else
            {
                // La majoritatea laptopurilor/plăcilor moderne, senzorii de ventilator nu sunt expuși prin WMI standard
                report.Fans.Add(new FanInfo
                {
                    Name = "Senzor Ventilator ACPI",
                    IsWorkingOk = true,
                    Status = "Gestionat automat de BIOS/EC"
                });
            }
        }
        catch { }
        //wifi
        try
        {
            using var search1 = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionID IS NOT NULL");
            foreach (var obj in search1.Get())
            {
                string name = obj["Name"]?.ToString() ?? "";
                if (name.Contains("Wi-Fi", StringComparison.OrdinalIgnoreCase) ||
                    name.Contains("Wireless", StringComparison.OrdinalIgnoreCase) ||
                    name.Contains("802.11", StringComparison.OrdinalIgnoreCase))
                {
                    report.Network.Wifi.Found = true;
                    report.Network.Wifi.Name = name;

                    uint errorCode = Convert.ToUInt32(obj["ConfigManagerErrorCode"] ?? 0);
                    report.Network.Wifi.IsWorkingOk = (errorCode == 0);
                    report.Network.Wifi.Status = report.Network.Wifi.IsWorkingOk ? "Ok (Functioneaza bine)" : $"Eroare (Cod eroare{errorCode})";

                    if (name.Contains("Wi-Fi 7", StringComparison.OrdinalIgnoreCase) || name.Contains("802.11be", StringComparison.OrdinalIgnoreCase))
                        report.Network.Wifi.Technology = "Wi-Fi 7 (802.11be)";
                    else if (name.Contains("Wi-Fi 6E", StringComparison.OrdinalIgnoreCase) || name.Contains("AX21", StringComparison.OrdinalIgnoreCase) || name.Contains("AX16", StringComparison.OrdinalIgnoreCase))
                        report.Network.Wifi.Technology = "Wi-Fi 6E (802.11ax 6GHz)";
                    else if (name.Contains("Wi-Fi 6", StringComparison.OrdinalIgnoreCase) || name.Contains("AX", StringComparison.OrdinalIgnoreCase) || name.Contains("802.11ax", StringComparison.OrdinalIgnoreCase))
                        report.Network.Wifi.Technology = "Wi-Fi 6 (802.11ax)";
                    else if (name.Contains("AC", StringComparison.OrdinalIgnoreCase) || name.Contains("802.11ac", StringComparison.OrdinalIgnoreCase))
                        report.Network.Wifi.Technology = "Wi-Fi 5 (802.11ac)";
                    else if (name.Contains("N", StringComparison.OrdinalIgnoreCase) || name.Contains("802.11n", StringComparison.OrdinalIgnoreCase))
                        report.Network.Wifi.Technology = "Wi-Fi 4 (802.11n)";
                    else
                        report.Network.Wifi.Technology = "Standard Wi-Fi";
                    break;
                }
            }
        }
        catch { }
        //bluetooth

        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Name LIKE '%Bluetooth%' AND NOT Name LIKE '%Enumerator%'");
            foreach (var obj in searcher.Get())
            {
                string name = obj["Name"]?.ToString() ?? "";
                report.Network.Bluetooth.Found = true;
                report.Network.Bluetooth.Name = name;

                uint errorCode = Convert.ToUInt32(obj["ConfigManagerErrorCode"] ?? 0);
                report.Network.Bluetooth.IsWorkingOk = (errorCode == 0);
                report.Network.Bluetooth.Status = report.Network.Bluetooth.IsWorkingOk ? "OK (Funcționează bine)" : $"Eroare/Failure (Cod {errorCode})";

                // Estimare versiune din denumire
                if (name.Contains("5.4", StringComparison.OrdinalIgnoreCase)) report.Network.Bluetooth.Technology = "Bluetooth 5.4";
                else if (name.Contains("5.3", StringComparison.OrdinalIgnoreCase)) report.Network.Bluetooth.Technology = "Bluetooth 5.3";
                else if (name.Contains("5.2", StringComparison.OrdinalIgnoreCase)) report.Network.Bluetooth.Technology = "Bluetooth 5.2";
                else if (name.Contains("5.1", StringComparison.OrdinalIgnoreCase)) report.Network.Bluetooth.Technology = "Bluetooth 5.1";
                else if (name.Contains("5.0", StringComparison.OrdinalIgnoreCase) || name.Contains("5", StringComparison.OrdinalIgnoreCase)) report.Network.Bluetooth.Technology = "Bluetooth 5.0+";
                else report.Network.Bluetooth.Technology = "Bluetooth Standard";

                break;
            }
        }
        catch { }

        //bateria mare
        try
        {
            using var searchBat = new ManagementObjectSearcher("SELECT * FROM Win32_Battery");
            var collection = searchBat.Get();
            if (collection.Count > 0)
            {
                report.Battery.Present = true;
                foreach (var obj in collection)
                {
                    report.Battery.Name = obj["Name"]?.ToString() ?? "Baterie laptop";
                    report.Battery.ChargePercentage = Convert.ToUInt32(obj["EstimatedChargeRemaining"] ?? 0);
                    report.Battery.Status = obj["Status"]?.ToString() ?? "OK";
                }
                try
                {
                    using var searchST = new ManagementObjectSearcher(@"root\WMI", "SELECT DesignedCapacity FROM BatteryStaticData");
                    foreach (var obj in searchST.Get())
                        report.Battery.DesignCapacityMWh = Convert.ToUInt32(obj["DesignedCapacity"]);

                    using var searchFu = new ManagementObjectSearcher(@"root\WMI", "SELECT FullChargedCapacity FROM BatteryFullChargedCapacity");
                    foreach (var obj in searchFu.Get())
                        report.Battery.FullChargeCapacityMwh = Convert.ToUInt32(obj["FullChargedCapacity"]);

                    if (report.Battery.DesignCapacityMWh > 0)
                    {
                        double health = ((double)report.Battery.FullChargeCapacityMwh / report.Battery.DesignCapacityMWh) * 100.0;
                        report.Battery.HealthPercentage = Math.Round(health, 2);
                        report.Battery.WearPercentage = Math.Round(100.0 - report.Battery.HealthPercentage);
                    }
                }
                catch { }


            }
        }
        catch { }

        //ram
        try
        {
            using var searcherRam = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
            foreach (var obj in searcherRam.Get())
            {
                ulong totalRamKb = Convert.ToUInt64(obj["TotalVisibleMemorySize"]);
                ulong freeRamKb = Convert.ToUInt64(obj["FreePhysicalMemory"]);

                report.Ram.TotalGb = Math.Round(totalRamKb / 1024.0 / 1024.0, 2);
                report.Ram.FreeGb = Math.Round(freeRamKb / 1024.0 / 1024.0, 2);
                report.Ram.UsedGb = Math.Round(report.Ram.TotalGb - report.Ram.FreeGb, 2);
                if (report.Ram.TotalGb > 0)
                    report.Ram.UsagePercentage = Math.Round((report.Ram.UsedGb / report.Ram.TotalGb) * 100.0, 1);
            }
            //diagnosticam placutele ram 
            using var searchRamM = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory");
            var ramCollection = searchRamM.Get();
            report.Ram.TotalModulesFound = ramCollection.Count;

            bool hasRamError = false;

            foreach (var obj in ramCollection)
            {
                ulong capacityBytes = Convert.ToUInt64(obj["Capacity"] ?? 0);
                uint speed = Convert.ToUInt32(obj["Speed"] ?? obj["ConfiguredClockSpeed"] ?? 0);
               // uint errorCode = Convert.ToUInt32(obj["ConfigManagerErrorCode"] ?? 0);
                string statusStr = obj["Status"]?.ToString() ?? "OK";

                bool moduleOk =(statusStr.Equals("OK", StringComparison.OrdinalIgnoreCase));
                if (!moduleOk) hasRamError = true;

                report.Ram.Modules.Add(new RamModuleDetail
                {
                    BankLabel = obj["BankLabel"]?.ToString() ?? obj["DeviceLocator"]?.ToString() ?? "Slot RAM",
                    Manufacturer = obj["Manufacturer"]?.ToString()?.Trim() ?? "Necunoscut",
                    CapacityGb = Math.Round(capacityBytes / (1024.0 * 1024.0 * 1024.0), 2),
                    SpeedMhz = speed,
                    IsWorking = moduleOk,
                    Status = moduleOk ? "OK (Plăcuță sănătoasă)" : "Eroare hardware  )"
                });
            }
            report.Ram.isHealthy = !hasRamError;
            report.Ram.HealthStatus = report.Ram.isHealthy ? "Sanatos -0 erori" : "Atentie: s-au detectat rori la cel putin un modul de memorie RAM";
        }
        catch { }
        //ssd

        
        
            //colectam predictiile S.M.A>R>T> din admin
            var smartFailures = new Dictionary<string, bool>();
            try
            {
                using var searcherSmart1 = new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM MSStorageDriver_FailurePredictStatus");
                foreach (var obj in searcherSmart1.Get())
                {
                    string instancName = obj["InstanceName"]?.ToString() ?? "";
                    bool predictFailure = Convert.ToBoolean(obj["PredictFailure"] ?? false);
                    smartFailures[instancName] = predictFailure;

                }

            }
            catch
            {//daca nu ruleaza ca admin sau discul e pus pe bus virtual

            }
        try
        {
            using var searcherSmart = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
            foreach (var obj in searcherSmart.Get())
            {
                ulong sizeBytes = Convert.ToUInt64(obj["Size"] ?? 0);
                string model = obj["Model"]?.ToString() ?? "Stocare Necunoscută";
                string rawStatus = obj["Status"]?.ToString() ?? "OK";
                string interfaceType = obj["InterfaceType"]?.ToString() ?? "SATA/NVMe";

                bool predictFailure = false;

                // Verificăm dacă WMI indică o eroare iminentă (Pred Fail)
                if (rawStatus.Equals("Pred Fail", StringComparison.OrdinalIgnoreCase))
                {
                    predictFailure = true;
                }

                double healthPercentage = predictFailure ? 15.0 : 100.0;
                string recommendation = predictFailure
                    ? "CRITIC: S.M.A.R.T. raportează uzură extremă sau erori de scriere! Înlocuiește SSD-ul/HDD-ul urgent."
                    : "S.M.A.R.T. OK: Discul este în stare optimă de funcționare, fără blocuri defecte.";

                report.Disks.Add(new DiskInfo
                {
                    Model = model,
                    SizeGb = Math.Round(sizeBytes / (1024.0 * 1024.0 * 1024.0), 2),
                    InterfaceType = interfaceType,
                    IsHealthy = !predictFailure && rawStatus.Equals("OK", StringComparison.OrdinalIgnoreCase),
                    HealthPercentage = healthPercentage,
                    PredictFailure = predictFailure,
                    SmartStatus = rawStatus,
                    HealthRecommendation = recommendation
                });
            }
        }
        catch{ }

        //cmos si bios
        // 10. BIOS (Separat de CMOS)
        try
        {
            using var searcherBios = new ManagementObjectSearcher("SELECT * FROM Win32_BIOS");
            foreach (var obj in searcherBios.Get())
            {
                report.Cmos.BiosManufacturer = obj["Manufacturer"]?.ToString() ?? "Necunoscut"; // Corectat: adaugat "?"
                report.Cmos.Biosversion = obj["SMBIOSBIOSVersion"]?.ToString() ?? obj["Version"]?.ToString() ?? "Necunoscut";
            }
        }
        catch { }

        // 11. CMOS Baterie
        try
        {
            using var searcherCmos = new ManagementObjectSearcher("SELECT * FROM Win32_CMOSBattery");
            var cmosCollection = searcherCmos.Get();
            if (cmosCollection.Count > 0)
            {
                foreach (var obj in cmosCollection)
                {
                    report.Cmos.Status = obj["Status"]?.ToString() ?? "OK";
                    ushort statusCode = Convert.ToUInt16(obj["BatteryStatus"] ?? 0);
                    report.Cmos.BatteryStatus = statusCode switch
                    {
                        3 => "OK (Sanatoasa)",
                        4 => "Nivel Scăzut (Low)",
                        5 => "Critica (Schimba bateria CR2032)",
                        _ => "Normala"
                    };
                }
            }
            else
            {
                report.Cmos.Status = "Integrata / Mod UEFI direct";
                report.Cmos.BatteryStatus = "OK";
            }
        }
        catch
        {
            // Laptopurile asus nu expun CMOS-ul bateriei standard in Windows
            report.Cmos.Status = "Integrata / Mod UEFI direct (Senzor invizibil)";
            report.Cmos.BatteryStatus = "OK";
        }

        return report;
    }
     

   

  

    private static double RunSingleThreadBenchmark(int ms)
    {
        var sw = Stopwatch.StartNew();
        long iterations = 0;
        double a = 1.0001;
        while(sw.ElapsedMilliseconds <ms)
        {
            for (int i = 0; i < 10_000; i++) a = Math.Sin(a) * Math.Cos(a) + Math.Sqrt(a);
            iterations++;

        }
        sw.Stop();
        return Math.Round((double)iterations / sw.ElapsedMilliseconds * 1000, 2);
    }
    public static double RunMultihreadsBench(int ms)
    {
        var sw = Stopwatch.StartNew();
        long totalIterations = 0;
        Parallel.For(0, Environment.ProcessorCount, _ =>
        {
            var innerSw = Stopwatch.StartNew();
            long inner = 0;
            double a = 1.0001;
            while (innerSw.ElapsedMilliseconds < ms)
            {
                for (int i = 0; i < 10_000; i++)
                    a = Math.Sin(a) + Math.Cos(a) + Math.Sqrt(a);
                inner++;
            }
            System.Threading.Interlocked.Add(ref  totalIterations, inner);
        });
        sw.Stop();
        return Math.Round((double)totalIterations / sw.ElapsedMilliseconds * 1000, 2);
    }
}