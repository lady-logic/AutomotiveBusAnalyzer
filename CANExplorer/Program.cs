using System.Runtime.InteropServices;

namespace CANExplorer
{
    class Program
    {
        // Vector XL API Konstanten
        private const int XL_SUCCESS = 0;
        private const int XL_CAN_MAX_DATA_LEN = 8;

        // Vector XL API Structures
        [StructLayout(LayoutKind.Sequential)]
        public struct XLCanMsg
        {
            public uint id;
            public ushort dlc;
            public ushort flags;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = XL_CAN_MAX_DATA_LEN)]
            public byte[] data;
            public ulong timeStamp;
        }

        // Vector XL API Imports 
        [DllImport("vxlapi64.dll", EntryPoint = "xlOpenDriver")]
        private static extern int xlOpenDriver();

        [DllImport("vxlapi64.dll", EntryPoint = "xlCloseDriver")]
        private static extern int xlCloseDriver();

        [DllImport("vxlapi64.dll", EntryPoint = "xlGetChannelMask")]
        private static extern int xlGetChannelMask(int hwType, int hwIndex, int hwChannel, ref ulong channelMask);

        [DllImport("vxlapi64.dll", EntryPoint = "xlCanReceive")]
        private static extern int xlCanReceive(int portHandle, ref uint messageCount, ref XLCanMsg msgBuffer);

        // Statistiken
        private static int totalMessages = 0;
        private static DateTime startTime = DateTime.Now;
        private static DateTime lastUpdate = DateTime.Now;

        static void Main(string[] args)
        {
            Console.WriteLine("==============================================");
            Console.WriteLine("CAN Explorer v0.1 - Hello Vector World!");
            Console.WriteLine("==============================================");
            Console.WriteLine();

            try
            {
                // Schritt 1: Vector Treiber öffnen
                Console.WriteLine("📡 Connecting to Vector driver...");
                int result = xlOpenDriver();

                if (result != XL_SUCCESS)
                {
                    Console.WriteLine("❌ Failed to open Vector driver!");
                    Console.WriteLine("Possible solutions:");
                    Console.WriteLine("   • Check if Vector hardware is connected");
                    Console.WriteLine("   • Try running as Administrator");
                    Console.WriteLine("   • Install Vector CANoe/CANalyzer for full driver support");
                    Console.WriteLine();
                    Console.WriteLine("Demo Mode: Simulating CAN messages...");
                    SimulateDemoMode();
                    return;
                }

                Console.WriteLine("Vector driver connected successfully!");
                Console.WriteLine();

                // Schritt 2: CAN Channel finden
                ulong channelMask = 0;
                result = xlGetChannelMask(-1, -1, 0, ref channelMask);

                if (result != XL_SUCCESS || channelMask == 0)
                {
                    Console.WriteLine("No CAN channels found - switching to demo mode");
                    SimulateDemoMode();
                    return;
                }

                Console.WriteLine($"Found CAN channels: 0x{channelMask:X}");
                Console.WriteLine();

                // Schritt 3: Nachrichtenschleife starten
                StartMessageLoop();
            }
            catch (DllNotFoundException)
            {
                Console.WriteLine("Vector XL API not found!");
                Console.WriteLine("Solutions:");
                Console.WriteLine("   • Make sure vxlapi64.dll is in system PATH");
                Console.WriteLine("   • Copy vxlapi64.dll to your exe directory");
                Console.WriteLine("   • Install Vector driver package completely");
                Console.WriteLine();
                Console.WriteLine("Running in Demo Mode instead...");
                SimulateDemoMode();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                Console.WriteLine("Running in Demo Mode...");
                SimulateDemoMode();
            }
        }

        private static void StartMessageLoop()
        {
            Console.WriteLine("Starting CAN message monitoring...");
            Console.WriteLine("Press [ESC] to stop");
            Console.WriteLine();
            Console.WriteLine("Live CAN Messages:");
            Console.WriteLine("----------------------------------------");

            while (true)
            {
                // ESC-Taste prüfen
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Escape)
                        break;
                }

                // CAN-Nachrichten empfangen
                XLCanMsg message = new XLCanMsg();
                message.data = new byte[XL_CAN_MAX_DATA_LEN];
                uint messageCount = 1;

                int result = xlCanReceive(0, ref messageCount, ref message);

                if (result == XL_SUCCESS && messageCount > 0)
                {
                    DisplayMessage(message);
                    totalMessages++;
                    UpdateStatistics();
                }

                Thread.Sleep(10); // Kurze Pause um CPU zu schonen
            }

            Console.WriteLine("\nMonitoring stopped by user");
            xlCloseDriver();
        }

        private static void SimulateDemoMode()
        {
            Console.WriteLine("SIMULATION MODE - Professional CAN Traffic Demo");
            Console.WriteLine("   Realistic automotive data patterns");
            Console.WriteLine("   Press [ESC] to stop, [SPACE] for burst mode");
            Console.WriteLine();
            Console.WriteLine("Simulated CAN Messages:");
            Console.WriteLine("----------------------------------------");

            Random random = new Random();

            // Typische Automotive CAN IDs mit realistischen Namen
            var canDatabase = new Dictionary<uint, string>
            {
                { 0x123, "Engine_Control_Unit" },
                { 0x456, "Battery_Management" },
                { 0x789, "Transmission_Data" },
                { 0x18F, "Vehicle_Speed_Sensor" },
                { 0x2A0, "Climate_Control" },
                { 0x3C1, "Door_Status" },
                { 0x4E2, "Diagnostic_Response" }
            };

            uint[] commonCanIds = canDatabase.Keys.ToArray();

            while (true)
            {
                // ESC-Taste prüfen
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Escape)
                        break;
                }

                // Zufällige CAN-Nachricht generieren
                XLCanMsg simulatedMessage = new XLCanMsg
                {
                    id = commonCanIds[random.Next(commonCanIds.Length)],
                    dlc = (ushort)random.Next(1, 9), // 1-8 Bytes
                    data = new byte[XL_CAN_MAX_DATA_LEN],
                    timeStamp = (ulong)DateTime.Now.Ticks
                };

                // Realistische Daten je nach CAN-ID
                GenerateRealisticData(simulatedMessage, random);

                DisplayMessage(simulatedMessage);
                totalMessages++;
                UpdateStatistics();

                // Realistische Timing (10-100ms zwischen Nachrichten)
                Thread.Sleep(random.Next(50, 200));
            }

            Console.WriteLine("\nDemo stopped by user");
        }

        private static void GenerateRealisticData(XLCanMsg message, Random random)
        {
            switch (message.id)
            {
                case 0x123: // Engine Data
                    message.data[0] = (byte)random.Next(50, 150);  // Temperature
                    message.data[1] = (byte)random.Next(0, 255);   // RPM Low
                    message.data[2] = (byte)random.Next(10, 50);   // RPM High
                    message.data[3] = (byte)random.Next(0, 200);   // Speed
                    break;

                case 0x456: // Battery Data
                    message.data[0] = (byte)random.Next(120, 140); // Voltage * 10
                    message.data[1] = (byte)random.Next(0, 100);   // Current
                    message.data[2] = (byte)random.Next(60, 100);  // State of Charge
                    break;

                default: // Generic data
                    for (int i = 0; i < message.dlc; i++)
                    {
                        message.data[i] = (byte)random.Next(0, 256);
                    }
                    break;
            }
        }

        private static void DisplayMessage(XLCanMsg message)
        {
            // Timestamp formatieren
            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");

            // CAN-ID formatieren
            string canId = $"0x{message.id:X3}";

            // Daten als Hex formatieren
            string hexData = "";
            for (int i = 0; i < message.dlc && i < message.data.Length; i++)
            {
                hexData += $"{message.data[i]:X2} ";
            }
            hexData = hexData.TrimEnd();

            // Nachricht anzeigen
            Console.WriteLine($"[{timestamp}] {canId}: [{hexData}] (DLC={message.dlc})");

            // Alle 10 Nachrichten: Zusätzliche Info anzeigen
            if (totalMessages % 10 == 0)
            {
                AddSmartComments(message);
            }
        }

        private static void AddSmartComments(XLCanMsg message)
        {
            // Einfache "Intelligenz" - interpretiere bekannte IDs
            switch (message.id)
            {
                case 0x123:
                    if (message.dlc >= 4 && message.data != null)
                    {
                        int rpm = (message.data[2] << 8) | message.data[1];
                        Console.WriteLine($"   Engine: ~{rpm * 4} RPM, Temp: ~{message.data[0]}°C, Speed: ~{message.data[3]} km/h");
                    }
                    break;

                case 0x456:
                    if (message.dlc >= 3 && message.data != null)
                    {
                        double voltage = message.data[0] / 10.0;
                        Console.WriteLine($"   Battery: {voltage:F1}V, Current: {message.data[1]}A, SoC: {message.data[2]}%");
                    }
                    break;

                case 0x789:
                    Console.WriteLine("   Vehicle Status Message");
                    break;

                default:
                    if (message.id >= 0x700)
                        Console.WriteLine("   Diagnostic Message");
                    break;
            }
        }

        private static void UpdateStatistics()
        {
            var now = DateTime.Now;

            // Alle 3 Sekunden: Statistiken updaten
            if ((now - lastUpdate).TotalSeconds >= 3)
            {
                var elapsed = now - startTime;
                double messagesPerSecond = totalMessages / elapsed.TotalSeconds;

                Console.WriteLine();
                Console.WriteLine($"Statistics: {totalMessages} messages total, {messagesPerSecond:F1} msg/sec");
                Console.WriteLine("----------------------------------------");

                lastUpdate = now;
            }
        }
    }
}