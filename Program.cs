using System.Net.NetworkInformation;
using System.Net;
using System.Diagnostics;
using System.Net.Sockets;
using System.Collections.Concurrent;
class Program
{

    static int ports_num = 65535;
    static IEnumerable<int> ports_range = Enumerable.Range(0, 1000);
    static string NL = Environment.NewLine; // shortcut
    static string NORMAL = Console.IsOutputRedirected ? "" : "\x1b[39m";
    static string RED = Console.IsOutputRedirected ? "" : "\x1b[91m";
    static string GREEN = Console.IsOutputRedirected ? "" : "\x1b[92m";
    static string YELLOW = Console.IsOutputRedirected ? "" : "\x1b[93m";
    static string BLUE = Console.IsOutputRedirected ? "" : "\x1b[94m";
    static string MAGENTA = Console.IsOutputRedirected ? "" : "\x1b[95m";
    static string CYAN = Console.IsOutputRedirected ? "" : "\x1b[96m";
    static string GREY = Console.IsOutputRedirected ? "" : "\x1b[97m";
    static string BOLD = Console.IsOutputRedirected ? "" : "\x1b[1m";
    static string NOBOLD = Console.IsOutputRedirected ? "" : "\x1b[22m";
    static string UNDERLINE = Console.IsOutputRedirected ? "" : "\x1b[4m";
    static string NOUNDERLINE = Console.IsOutputRedirected ? "" : "\x1b[24m";
    static string REVERSE = Console.IsOutputRedirected ? "" : "\x1b[7m";
    static string NOREVERSE = Console.IsOutputRedirected ? "" : "\x1b[27m";
    static async Task Main(string[] args)
    {

        print_welcome_message();
        string input = "";
        IPAddress ip_address = IPAddress.None;
        string address_input;
        int degrees;
        var openports = new Dictionary<String, List<int>>();
        while (true)
        {
            // Console.Clear();
            Console.WriteLine($"type a file path or a an ipv4 address: {input}");
            input = Console.ReadLine();
            if (String.IsNullOrWhiteSpace(input))
            {
                continue;
            }
            if (input == "exit")
            {
                printPingu("bye_pingu.txt");
                return;
            }
            try
            {
                address_input = input.ToString().Split(' ')[0];
                degrees = int.Parse(input.ToString().Split(' ')[1]);
            }
            catch
            {
                address_input = input.ToString();
                degrees = 20;
            }

            Console.WriteLine($"adress:{address_input} degress:{degrees}");
            if (IPAddress.TryParse(address_input, out ip_address))
            {
                if (!Ping(ip_address))
                {
                    Console.Write($"{RED}");
                    printstaticPingu("error_pingu.txt");
                    Console.Write($"{NORMAL}");
                    Console.WriteLine($"address {BLUE}{ip_address}{NORMAL} was not reachable");
                    continue;
                }

                await pingports(openports, ip_address, degrees);
                printopenports(openports);

            }
            //scan IPAddress of file
            else if (File.Exists(address_input))
            {
                Console.WriteLine($"Pinging files in: {address_input}\n\n\n");
                var file = File.ReadAllLines(address_input);
                var NR_of_addresses = file.Length;
                var sw = Stopwatch.StartNew();
                foreach (string address in file)
                {
                    var (pinged, ipaddress) = Ping(address);
                    if (!pinged)
                    {
                        openports.Add(address, [-1]);
                        continue;
                    }

                    await pingports(openports, ipaddress, degrees);
                    Thread.Sleep(1000);
                }
                sw.Stop();
                printopenports(openports);
                Console.WriteLine($"Elapsed: {sw.Elapsed.ToString()} ms");
            }
            else
            {
                Console.Clear();
                Console.Write($"{RED}");
                printstaticPingu("error_pingu.txt");
                Console.Write($"{NORMAL}");
                Console.WriteLine($"{RED}Invalid input: {input.ToString()} {NORMAL} please type in a valid file name or IPAddress");
                continue;
            }
        }

    }

    public static void printopenports(Dictionary<string, List<int>> openPorts)
    {
        Console.WriteLine("Printing open ports:\n");

        const int columnWidth = 20;

        // Print headers (keys) with fixed width
        int key_index = 0;
        foreach (var key in openPorts.Keys)
        {

            Console.Write($"{key.PadRight(key.ToString().Length+columnWidth)}");
            key_index++;
        }
        Console.WriteLine();

        int maxPorts = openPorts.Values.Max(list => list.Count);

        for (int i = 0; i < maxPorts; i++)
        {
            int index = 0;
            foreach (var key in openPorts.Keys)
            {
                var ports = openPorts[key];
                if (i < ports.Count)
                    Console.Write($"{GREEN}{ports[i].ToString().PadRight(key.ToString().Length+columnWidth)}{NORMAL}");
                else
                    Console.Write(new string(' ', key.ToString().Length+columnWidth)); // Empty space
                index++;
            }
            Console.WriteLine();
        }
            Console.WriteLine();
            Console.WriteLine();
    }



    public static bool Ping(IPAddress address)
    {
        Ping ping = new Ping();
        bool pinged = false;
        try
        {
            PingReply reply = ping.Send(address);

            if (reply.Status == IPStatus.Success)
            {
                Console.WriteLine($"Ping to {GREEN}{address}successful!{NORMAL}");
                Console.WriteLine($"Roundtrip time: {reply.RoundtripTime}ms");
                Console.WriteLine($"Address: {reply.Address}");
                Console.WriteLine($"message: {reply.GetType()}");
                pinged = true;
            }
            else
            {
                Console.WriteLine($"Ping failed: {reply.Status}");
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ping error: {ex.Message}");
        }


        return pinged;
    }

    public static (bool, IPAddress) Ping(string address)
    {
        Ping ping = new Ping();
        bool pinged = false;
        var ret_address = IPAddress.None;
        try
        {
            PingReply reply = ping.Send(address);
            if (reply.Status == IPStatus.Success)
            {
                Console.WriteLine($"Ping to {GREEN}{address} successful!{NORMAL}");
                Console.WriteLine($"Roundtrip time: {reply.RoundtripTime}ms");
                Console.WriteLine($"Address: {reply.Address}");
                Console.WriteLine($"message: {reply.GetType()}");
                pinged = true;
                ret_address = reply.Address;
            }
            else
            {
                Console.Write($"{RED}");
                printstaticPingu("error_pingu.txt");
                Console.Write($"{NORMAL}");
                Console.WriteLine($"Ping failed: {reply.Status}");
            }

        }
        catch (Exception ex)
        {
            Console.Write($"{RED}");
            printstaticPingu("error_pingu.txt");
            Console.Write($"{NORMAL}");
            Console.WriteLine($"Ping error: {ex.Message}");
        }

        return (pinged, ret_address);
    }

    public async static Task pingports(Dictionary<string, List<int>> dict, IPAddress ip_address, int degrees)
    {

        var openPorts = new ConcurrentBag<int>();
        await Parallel.ForAsync(0, ports_num + 1, new ParallelOptions { MaxDegreeOfParallelism = degrees }, async (port, _) =>
        {
            var t = await isportopen(ip_address.ToString(), port);
            if (t)
            {
                openPorts.Add(port);
            }
        });

        dict[ip_address.ToString()] = openPorts.ToList();
    }

    public async static Task<bool> isportopen(string Address, int port)
    {
        try
        {

            using var client = new TcpClient();
            var connectTask = client.ConnectAsync(Address, port);

            var timeoutTask = Task.Delay(3000);
            var completedTask = await Task.WhenAny(connectTask, timeoutTask);

            bool connected = completedTask == connectTask && client.Connected;
            client.Close();
            return connected;
        }
        catch (Exception e)
        {
            Console.WriteLine("error: " + e + "\n" + e.StackTrace);
            Console.WriteLine("failed at port " + Address + ":" + port);
            return false;
        }

    }

    public static void print_welcome_message()
    {
        int width = Console.WindowWidth;
        int height = Console.WindowHeight;
        Console.Clear();

        for (int i = 0; i < height / 2 - 1; i++)
        {
            Console.WriteLine();
        }
        for (int i = 0; i < (width / 2) - 20; i++)
        {
            Console.Write(" ");
        }
        Console.WriteLine($"Welcome to {BLUE}Pingu{NORMAL}, a Ping-List-Bot:");
        for (int i = 0; i < (width / 2) - 15; i++)
        {
            Console.Write(" ");
        }

        Console.WriteLine($"press {GREEN}ENTER{NORMAL} to continue");
        Console.Read();
        Console.Clear();
        printPingu("hello_pingu.txt");
    }

    public static void printPingu(string file)
    {
        Console.Clear();
        //chatgpt generated
        List<string> lines = new List<string>(File.ReadLines(file));
        int consoleHeight = Console.WindowHeight;

        for (int i = 0; i < lines.Count + consoleHeight; i++)
        {
            Console.Clear();

            int paddingLines = consoleHeight - i;
            if (paddingLines > 0)
            {
                for (int j = 0; j < paddingLines; j++)
                {
                    Console.WriteLine();
                }
            }
            for (int j = Math.Max(0, i - consoleHeight + 1); j <= i && j < lines.Count; j++)
            {
                Console.WriteLine(lines[j]);
            }

            Thread.Sleep(1000 / 30);
        }
    }

    public static void printstaticPingu(string pingu_file)
    {
        var file = File.ReadLines(pingu_file);
        foreach (string line in file)
        {
            Console.WriteLine(line);
        }
    }


}

