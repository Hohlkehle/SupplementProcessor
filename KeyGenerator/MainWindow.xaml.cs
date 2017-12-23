using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AppSoftware.LicenceEngine.Common;
using AppSoftware.LicenceEngine.KeyVerification;
using AppSoftware.LicenceEngine.KeyGenerator;
using System.Security.Cryptography;

using SQLite;
using Stocks;
using Path = System.IO.Path;

namespace KeyGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string SALT1 = "W8fd6";
        const string SALT2 = "_r6Er7Q";
        const int SETLEN = 16;
        KeyByteSet[] keyByteSets = new[]
        {
            new KeyByteSet(1, 46, 192, 9),
            new KeyByteSet(2, 16, 45, 13),
            new KeyByteSet(3, 147, 161, 5), 
            new KeyByteSet(4, 10, 253, 8),
            new KeyByteSet(5, 180, 73, 2),
            new KeyByteSet(6, 125, 23, 78), 
            new KeyByteSet(7, 81, 75,124), 
            new KeyByteSet(8, 60, 8, 32),
            new KeyByteSet(9, 58, 6, 97),
            new KeyByteSet(10, 96, 254, 23),
            new KeyByteSet(11, 44, 20, 1),
            new KeyByteSet(12, 12, 73, 134), 
            new KeyByteSet(13, 210, 52, 14), 
            new KeyByteSet(14, 160, 80, 36),
            new KeyByteSet(15, 50, 126, 197),
            new KeyByteSet(16, 6, 254, 223),
        };

        PkvLicenceKeyGenerator pkvLicenceKey;
        public class Program
        {
            Database _db;

            public void Initialize()
            {
                var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Stocks.db");
                _db = new Database(dbPath);
            }

            public void DisplayStock(string stockSymbol)
            {
                var stock = _db.QueryStock(stockSymbol);

                if (stock == null)
                {
                    Console.WriteLine("I don't know about {0}", stockSymbol);
                    Console.WriteLine("Run \"up {0}\" to update the stock", stockSymbol);
                }
                else
                {

                    //
                    // Display the last 1 week
                    //				
                    foreach (var v in _db.QueryValuations(stock))
                    {
                        Console.WriteLine("  {0}", v);
                    }

                }
            }

            public void UpdateStock(string stockSymbol)
            {
                _db.UpdateStock(stockSymbol);
            }

            public void ListStocks()
            {
                foreach (var stock in _db.QueryAllStocks())
                {
                    Console.WriteLine(stock);
                }
            }

            public void DisplayBanner()
            {
                Console.WriteLine("Stocks - a demo of sqlite-net");
                //Console.WriteLine("Using " + _db.Database);
                Console.WriteLine();
            }

            public void DisplayHelp(string cmd)
            {
                Action<string, string> display = (c, h) => { Console.WriteLine("{0} {1}", c, h); };
                var cmds = new SortedDictionary<string, string> {
				{
					"ls",
					"\t List all known stocks"
				},
				{
					"exit",
					"\t Exit stocks"
				},
				{
					"up stock",
					"Updates stock"
				},
				{
					"help",
					"\t Displays help"
				},
				{
					"stock",
					"\t Displays latest valuations for stock"
				}
			};
                if (cmds.ContainsKey(cmd))
                {
                    display(cmd, cmds[cmd]);
                }
                else
                {
                    foreach (var ch in cmds)
                    {
                        display(ch.Key, ch.Value);
                    }
                }
            }

            public void Run()
            {
                var WS = new char[] {
				' ',
				'\t',
				'\r',
				'\n'
			};

                Initialize();

                DisplayBanner();
                //DisplayHelp("");

                for (; ; )
                {
                    Console.Write("$ ");
                    var cmdline = Console.ReadLine();

                    var args = cmdline.Split(WS, StringSplitOptions.RemoveEmptyEntries);
                    if (args.Length < 1)
                        continue;
                    var cmd = args[0].ToLowerInvariant();

                    if (cmd == "?" || cmd == "help")
                    {
                        DisplayHelp("");
                    }
                    else if (cmd == "exit")
                    {
                        break;
                    }
                    else if (cmd == "ls")
                    {
                        ListStocks();
                    }
                    else if (cmd == "up")
                    {
                        if (args.Length == 2)
                        {
                            UpdateStock(args[1].ToUpperInvariant());
                        }
                        else
                        {
                            DisplayHelp("up stock");
                        }
                    }
                    else
                    {
                        DisplayStock(cmd.ToUpperInvariant());
                    }
                }
            }
        }
        public MainWindow()
        {
            //new Program().Run();

            InitializeComponent();

            pkvLicenceKey = new PkvLicenceKeyGenerator();
        }

        internal int GetUniqueID(string username)
        {
            SHA1 sha = new SHA1CryptoServiceProvider();
            SHA256 sha256 = new SHA256CryptoServiceProvider();
            return GetString(sha256.ComputeHash(sha.ComputeHash(GetBytes(SALT1 + username + SALT2)))).GetHashCode();
        }

        private void ButtonGenerate_Click(object sender, RoutedEventArgs e)
        {
            var username = TextBoxName.Text;
            if (username == string.Empty)
            {
                return;
            }

            var seed = GetUniqueID(username);
            var key = pkvLicenceKey.MakeKey(seed, keyByteSets);

            TextBoxCode.Text = key;
        }

        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }
    }
}
