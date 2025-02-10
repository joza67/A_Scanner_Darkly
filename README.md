# A Scanner Darkly

**A Scanner Darkly** is a C# console application offering basic network utilities:

1. **Ping an Address:** Send ICMP ping requests to a specified IP address or hostname to check its availability and response time.

2. **Scan IP Range:** Scan a specified range of IP addresses to identify active hosts within that range.

3. **Select Text Color:** Customize the console text color for better readability or personal preference.

## Requirements

- .NET SDK 6.0 or later

## Installation

1. **Clone the repository:**

   ```bash
   git clone https://github.com/your-username/a-scanner-darkly.git
   cd a-scanner-darkly

   Build and run the application:

bash
Copy
Edit
dotnet build
dotnet run
Note: If you don't have the .NET SDK installed, download and install it from the official Microsoft website.



Upon running the application, you'll be presented with a main menu offering the following options:

Ping an address: Enter the IP address or hostname you wish to ping. The application will continuously send ping requests until you press the spacebar to stop.

Scan IP range: Specify the start and end IP addresses of the range you want to scan. The application will display all active IP addresses within that range.

Select text color: Choose your preferred console text color from the provided options.

Exit: Close the application.

