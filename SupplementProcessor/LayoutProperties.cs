using SupplementProcessor.Data;
using SupplementProcessor.UserControls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;

namespace SupplementProcessor
{
    [Serializable]
    public class LayoutProperties
    {
        public List<ZSumbolInfo> m_ZSumbolInfo;
        public List<GuideLineInfo> m_GuideLineInfo;
        public List<CaptionInfo> m_CaptionInfo;
        public List<TableInfo> m_Table;

        [XmlIgnore]
        public const string FRONT_SIDE_CONFIG_FILE = "layout.xml";
        [XmlIgnore]
        public const string REAR_SIDE_CONFIG_FILE = "layout-rear.xml";
        [XmlAttribute]
        public string Name { set; get; }
        [XmlIgnore]
        public byte[] BackgroundImage { set; get; }
        [XmlAttribute]
        public LayoutType LayoutType { set; get; }
        public Point Offset { set; get; }
        public Point BackgroundOffset { set; get; }
        public Point Size { set; get; }
        [XmlIgnore]
        public Point OffsetInPixel
        {
            get { return new Point((Offset.X * MainWindow.DpiX / 2.54d / 10d), Math.Ceiling(Offset.Y * MainWindow.DpiY / 2.54d / 10d)); }
            set { Offset = new Point((value.X / MainWindow.DpiX * 2.54d * 10d), (value.Y / MainWindow.DpiY * 2.54d * 10d)); }
        }

        public LayoutProperties() { }

        internal void Check()
        {
            //m_CaptionInfo = m_CaptionInfo.Where((e) => { if (e.CaptionText == "") e.CaptionText = "corrupted"; return true; }).ToList<CaptionInfo>();

            //foreach (var e in m_CaptionInfo)
            //{
            //    if (e.CaptionText == "")
            //        e.CaptionText = "corrupted";
            //}

            m_CaptionInfo.RemoveAll((e) => e.CaptionText == "");
        }

        [Obsolete]
        public void Save(string filename)
        {
            File.Delete(filename);

            using (Stream writer = new FileStream(filename, FileMode.OpenOrCreate))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(LayoutProperties));
                    serializer.Serialize(writer, this);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        [Obsolete]
        public static void Save(LayoutProperties prop, string fileName)
        {
            prop.Save(fileName);
        }

        [Obsolete]
        public void SetBackgroundImage(string fileName)
        {
            BackgroundImage = LayoutFileReader.ImageToByte(new System.Drawing.Bitmap(fileName));
        }

        [Obsolete]
        public static string GetConfigFilename(LayoutSide side)
        {
            if (side == LayoutSide.Front)
                return FRONT_SIDE_CONFIG_FILE;

            return REAR_SIDE_CONFIG_FILE;
        }

     
    }
}
