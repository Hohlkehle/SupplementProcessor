using LayoutFileSystem;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace LayoutFileSystemTest
{
    class Program
    {
        const string DEFAULT_EXTENSION = ".splt";
        const string FRONT_LAYOUT = "layout-front.xml";
        const string REAR_LAYOUT = "layout-rear.xml";
        const string FRONT_BACKGROUND = "background.jpg";
        const string REAR_BACKGROUND = "foreground.jpg";

        static void Main(string[] args)
        {
            //args = new string[1];
            //args[0] = @"U:\Desktop\Visual Studio Project\Audio\ShowComposer\ShowComposer\bin\Debug\scenario 1.scomp";
            if (args.Length == 0)
            {
                Console.WriteLine("Supplement project file manager (packer/unpacker).");
                Console.WriteLine("Usage: ");
                Console.WriteLine("Drag .splt file to spfm.exe for extract.");
                Console.WriteLine("Drag directory with extracted files to spfm.exe for pack.");
            }
            else 
            {
                //var file = Path.Combine(Directory.GetCurrentDirectory(), "scenario 1.scomp");
                if (IsDirectory(File.GetAttributes(args[0])))
                {
                    Pack(args[0]);
                }
                else
                {
                    Unpack(args[0]);
                }
            }

            Console.WriteLine("Done...");
            System.Threading.Thread.Sleep(1000);
            
            //using (ZipArchive zip = ZipFile.Open("test.zip", ZipArchiveMode.Create))
            //{
            //    zip.CreateEntryFromFile(@"c:\something.txt", "data/path/something.txt");
            //}
        }

        static void Pack(string path)
        {
            var di = new DirectoryInfo(path);
            var name = di.Name;
            var file = Path.Combine(path.Split(new string[] { name }, StringSplitOptions.RemoveEmptyEntries)[0], name + DEFAULT_EXTENSION);

            if (File.Exists(file))
            {
                File.Delete(file);
            }

            var xml1file = Path.Combine(path, FRONT_LAYOUT);
            var xml2file = Path.Combine(path, REAR_LAYOUT);
            var jpg1file = Path.Combine(path, FRONT_BACKGROUND);
            var jpg2file = Path.Combine(path, REAR_BACKGROUND);

            var xml1file_text = GetBytes(File.ReadAllText(xml1file));
            var xml2file_text = GetBytes(File.ReadAllText(xml2file));
            var jpg1file_image = ImageToByte(new Bitmap(jpg1file));
            var jpg2file_image = ImageToByte(new Bitmap(jpg2file));

            var layout = new Layout();

            layout.Data1 = xml1file_text;
            layout.Data2 = xml2file_text;
            layout.Data3 = jpg1file_image;
            layout.Data4 = jpg2file_image;

            layout.Save(file);
        }

        static void Unpack(string file)
        {
            var dir = Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file));

            if (Directory.Exists(dir))
            {
                var di = new DirectoryInfo(dir);

                foreach (var f in di.GetFiles())
                    f.Delete();
                
                foreach (var d in di.GetDirectories())
                    d.Delete(true);
            }
            else
            {
                Directory.CreateDirectory(dir);
            }

            try
            {
                var layout = new Layout(file);

                var data1 = GetString(layout.Data1);
                var data2 = GetString(layout.Data2);
                var data3 = GetImage(layout.Data3);
                var data4 = GetImage(layout.Data4);

                using (StreamWriter sw = new StreamWriter(File.Open(Path.Combine(dir, FRONT_LAYOUT), FileMode.Create), Encoding.UTF8))
                    sw.Write(data1);
                using (StreamWriter sw = new StreamWriter(File.Open(Path.Combine(dir, REAR_LAYOUT), FileMode.Create), Encoding.UTF8))
                    sw.Write(data2);

                data3.Save(Path.Combine(dir, FRONT_BACKGROUND), ImageFormat.Jpeg);
                data4.Save(Path.Combine(dir, REAR_BACKGROUND), ImageFormat.Jpeg); 

            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine("Layout file " + file + " cannot be loaded!");
                Console.WriteLine();
                Console.Write(e.ToString());
            }
            catch (IOException e)
            {
                Console.WriteLine("Layout file " + file + " cannot be loaded!");
                Console.WriteLine();
                Console.Write(e.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("Layout file " + file + " cannot be loaded!");
                Console.WriteLine();
                Console.Write(e.ToString());
            }
        }

        static bool IsDirectory(FileAttributes attr )
        {
            return ((attr & FileAttributes.Directory) == FileAttributes.Directory);
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

        public static byte[] ImageToByte(Image img)
        {
            if (img.Width == 1 && img.Height == 1)
            {
                return new byte[] { };
            }
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }

        public static Image GetImage(byte[] bytes)
        {
            if (bytes.Length == 0)
                return new Bitmap(1, 1);
             return Image.FromStream(new MemoryStream(bytes));
            //using (Image image = Image.FromStream(new MemoryStream(bytes)))
            //{
            //    image.Save("output.jpg", ImageFormat.Jpeg);  // Or Png
            //}
        }
    }
}
