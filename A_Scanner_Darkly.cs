using System; // Import basic classes for working with input/output and general types.
using System.Collections.Generic; // Import classes for collections like lists.
using System.Linq; // Import classes and methods for LINQ queries.
using System.Net; // Import classes for network operations, such as IP address handling.
using System.Net.NetworkInformation; // Import classes for network-related operations, such as pinging.
using System.Text; // Import classes for text encoding.
using System.Threading; // Import classes for threading and synchronization.
using System.Threading.Tasks; // Import classes for asynchronous programming.

namespace NetworkUtilities // Define a namespace to organize code and prevent name conflicts.
{
    class Program // Define a class named Program.
    {
        // Static variable to hold the current console text color.
        static ConsoleColor currentColor = ConsoleColor.White; // Default text color is set to White.

        static void Main(string[] args) // Main method, the entry point of the application.
        {
            // Infinite loop to keep the application running until the user chooses to exit.
            while (true)
            {
                Console.Clear(); // Clear the console screen.
                Console.ForegroundColor = currentColor; // Set the text color based on the currentColor variable.

                // Display the menu options to the user.
                Console.WriteLine("");
                Console.WriteLine("A Scanner Darkly:");
                Console.WriteLine("");
                Console.WriteLine("Made on 31/07/2024 with great help of ChatGPT 4.0");
                Console.WriteLine("");
                Console.WriteLine("1. Ping an address");
                Console.WriteLine("2. Scan IP range");
                Console.WriteLine("3. Select text color");
                Console.WriteLine("4. Exit");
                Console.Write("Select an option: ");

                // Read the user's choice from the console.
                string? choice = Console.ReadLine();

                // Use a switch statement to handle different menu options.
                switch (choice)
                {
                    case "1":
                        PingAddress(); // Call method to ping an address.
                        break;
                    case "2":
                        ScanIPRange(); // Call method to scan an IP range.
                        break;
                    case "3":
                        SelectTextColor(); // Call method to change text color.
                        break;
                    case "4":
                        return; // Exit the loop and end the application.
                    default:
                        Console.WriteLine("Invalid option. Please select again."); // Handle invalid options.
                        break;
                }
            }
        }

        // Method to ping a specified IP address or hostname.
        static void PingAddress()
        {
            Console.Clear(); // Clear the console screen.
            Console.ForegroundColor = currentColor; // Set the text color based on the currentColor variable.

            // Prompt user for the IP address or hostname to ping.
            Console.Write("Enter the IP address or hostname to ping: ");
            string? address = Console.ReadLine();

            // Prompt user for the delay between pings, default to 1000 milliseconds if invalid.
            Console.Write("Enter the number of milliseconds between pings: ");
            if (!int.TryParse(Console.ReadLine(), out int delay))
            {
                Console.WriteLine("Invalid input. Using default delay of 1000ms.");
                delay = 1000;
            }

            // Display information about the smallest and largest possible buffer sizes.
            Console.WriteLine("Enter the number of bytes to send (default is 32): ");
            Console.WriteLine("Minimum buffer size: 1 byte");
            Console.WriteLine("Maximum buffer size: 65,507 bytes");

            // Prompt user for the buffer size in bytes, default to 32 bytes if invalid.
            if (!int.TryParse(Console.ReadLine(), out int bufferSize) || bufferSize < 1 || bufferSize > 65507)
            {
                Console.WriteLine("Invalid input or out of range. Using default buffer size of 32 bytes.");
                bufferSize = 32;
            }

            // Create a buffer of the specified size and fill it with random data.
            byte[] buffer = new byte[bufferSize];
            new Random().NextBytes(buffer);

            // Create a Ping object to send ping requests.
            Ping pingSender = new Ping();
            Console.WriteLine("Pinging started. Press SPACE to stop...");

            // Start a new task to continuously ping the address.
            Task.Run(() =>
            {
                while (true)
                {
                    // Check if a key has been pressed and if it's the SPACE key to stop pinging.
                    if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Spacebar)
                    {
                        Console.WriteLine("Pinging stopped.");
                        break;
                    }

                    try
                    {
                        // Use "localhost" as a default address if user input is null.
                        if (address == null)
                        {
                            address = "localhost";
                        }

                        // Send a ping request to the specified address with a 1000 ms timeout.
                        PingReply reply = pingSender.Send(address, 1000, buffer);

                        // Get the current time for timestamp.
                        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                        // Check if the ping was successful and print the results.
                        if (reply.Status == IPStatus.Success)
                        {
                            Console.WriteLine($"[{timestamp}] Reply from {reply.Address}: bytes={reply.Buffer.Length} time={reply.RoundtripTime}ms TTL={reply.Options.Ttl} (Delay: {delay}ms)");
                        }
                        else
                        {
                            Console.WriteLine($"[{timestamp}] Ping failed: {reply.Status}");
                        }
                    }
                    catch (Exception ex)
                    {
                        // Print any exceptions that occur during pinging.
                        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Ping error: {ex.Message}");
                    }

                    // Wait for the specified delay before sending the next ping.
                    Thread.Sleep(delay);
                }
            }).Wait(); // Wait for the task to complete.
        }

        // Method to scan a range of IP addresses.
        static void ScanIPRange()
        {
            Console.Clear(); // Clear the console screen.
            Console.ForegroundColor = currentColor; // Set the text color based on the currentColor variable.

            try
            {
                // Prompt user for the start and end IP addresses.
                Console.Write("Enter the start IP address: ");
                string? startIP = Console.ReadLine();
                Console.Write("Enter the end IP address: ");
                string? endIP = Console.ReadLine();

                // Parse the IP addresses and check if they are valid.
                if (!IPAddress.TryParse(startIP, out IPAddress? startIPAddress) || !IPAddress.TryParse(endIP, out IPAddress? endIPAddress))
                {
                    Console.WriteLine("Invalid IP address range.");
                    return;
                }

                // Convert IP addresses to 32-bit unsigned integers for comparison.
                uint start = BitConverter.ToUInt32(startIPAddress.GetAddressBytes().Reverse().ToArray(), 0);
                uint end = BitConverter.ToUInt32(endIPAddress.GetAddressBytes().Reverse().ToArray(), 0);

                // Ensure the start address is less than or equal to the end address.
                if (start > end)
                {
                    Console.WriteLine("Invalid IP range. Start address should be less than or equal to the end address.");
                    return;
                }

                // Inform the user that the scan is starting.
                Console.WriteLine($"Scanning range {startIP} - {endIP}...");

                // Determine the maximum level of parallelism based on the number of processor cores.
                int maxParallelism = Environment.ProcessorCount * 10;

                // Create a semaphore to limit the number of concurrent tasks.
                SemaphoreSlim semaphore = new SemaphoreSlim(maxParallelism);

                // Create a list to hold the tasks for scanning IP addresses.
                List<Task> tasks = new List<Task>();

                // Create a list to store active IP addresses and their host names.
                List<(string IP, string HostName)> activeIPs = new List<(string, string)>();

                // Loop through the IP address range and create a task for each IP address.
                for (uint i = start; i <= end; i++)
                {
                    semaphore.Wait(); // Wait for a semaphore slot to become available.

                    uint ipAddressValue = i;
                    Task task = Task.Run(async () =>
                    {
                        // Convert the integer value to an IPAddress object.
                        IPAddress address = new IPAddress(BitConverter.GetBytes(ipAddressValue).Reverse().ToArray());

                        try
                        {
                            // Create a Ping object to send ping requests.
                            Ping pingSender = new Ping();
                            int successfulPings = 0;

                            // Send multiple pings to check for successful replies.
                            for (int j = 0; j < 5; j++)
                            {
                                try
                                {
                                    PingReply reply = await pingSender.SendPingAsync(address, 100);
                                    if (reply.Status == IPStatus.Success)
                                    {
                                        successfulPings++;
                                    }
                                }
                                catch (PingException pex)
                                {
                                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Ping error for {address}: {pex.Message}");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Unexpected error for {address}: {ex.Message}");
                                }
                            }

                            // If there were successful pings, resolve the hostname and add to the list.
                            if (successfulPings > 0)
                            {
                                string hostName = "Unknown";
                                try
                                {
                                    hostName = Dns.GetHostEntry(address).HostName;
                                }
                                catch (Exception dnsEx)
                                {
                                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] DNS resolution error for {address}: {dnsEx.Message}");
                                }

                                // Lock the list to ensure thread safety while adding items.
                                lock (activeIPs)
                                {
                                    activeIPs.Add((address.ToString(), hostName));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Error while processing {address}: {ex.Message}");
                        }
                        finally
                        {
                            semaphore.Release(); // Release the semaphore slot after the task is done.
                        }
                    });

                    // Add the task to the list of tasks.
                    tasks.Add(task);
                }

                // Wait for all tasks to complete.
                Task.WhenAll(tasks).Wait();

                // Sort the list of active IPs by the last byte of the IP address.
                var sortedActiveIPs = activeIPs.OrderBy(ip => IPAddress.Parse(ip.IP).GetAddressBytes().Last()).ToList();

                // Display the list of active IP addresses and their host names.
                Console.WriteLine("");
                Console.WriteLine("");
                Console.WriteLine("Scan complete. Active IP addresses:\n");
                Console.WriteLine("");
                Console.WriteLine("");

                // Display the active IP addresses.
                foreach (var ip in sortedActiveIPs)
                {
                    Console.WriteLine($"Active IP: {ip.IP} - {ip.HostName}");
                }

                // Display the total number of active hosts.
                Console.WriteLine($"\nTotal number of active hosts: {sortedActiveIPs.Count}");

                // Prompt user to press Enter to return to the menu.
                Console.WriteLine("\n\nPress Enter to return to the menu...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                // Print any exceptions that occur during the IP range scan.
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] An error occurred: {ex.Message}");
            }
        }

        // Method to change the text color of the console.
        static void SelectTextColor()
        {
            Console.Clear(); // Clear the console screen.
            Console.ForegroundColor = ConsoleColor.White; // Reset text color to White.

            // Display the available color options to the user.
            Console.WriteLine("Select text color:");
            Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine("1. Red");
            Console.ForegroundColor = ConsoleColor.Green; Console.WriteLine("2. Green");
            Console.ForegroundColor = ConsoleColor.Blue; Console.WriteLine("3. Blue");
            Console.ForegroundColor = ConsoleColor.Yellow; Console.WriteLine("4. Yellow");
            Console.ForegroundColor = ConsoleColor.Cyan; Console.WriteLine("5. Cyan");
            Console.ForegroundColor = ConsoleColor.Magenta; Console.WriteLine("6. Magenta");
            Console.ForegroundColor = ConsoleColor.Gray; Console.WriteLine("7. Gray");
            Console.ForegroundColor = ConsoleColor.White; Console.WriteLine("8. White");
            Console.Write("Select a color (1-8): ");

            // Read the user's color choice.
            string? colorChoice = Console.ReadLine();

            // Set the currentColor variable based on the user's choice.
            switch (colorChoice)
            {
                case "1":
                    currentColor = ConsoleColor.Red;
                    break;
                case "2":
                    currentColor = ConsoleColor.Green;
                    break;
                case "3":
                    currentColor = ConsoleColor.Blue;
                    break;
                case "4":
                    currentColor = ConsoleColor.Yellow;
                    break;
                case "5":
                    currentColor = ConsoleColor.Cyan;
                    break;
                case "6":
                    currentColor = ConsoleColor.Magenta;
                    break;
                case "7":
                    currentColor = ConsoleColor.Gray;
                    break;
                case "8":
                    currentColor = ConsoleColor.White;
                    break;
                default:
                    Console.WriteLine("Invalid choice. Using default color.");
                    currentColor = ConsoleColor.White;
                    break;
            }

            // Set the text color to the selected color and notify the user.
            Console.ForegroundColor = currentColor;
            Console.WriteLine($"Text color changed to {currentColor}.");
            Console.WriteLine("\nPress Enter to return to the menu...");
            Console.ReadLine();
        }
    }
}
