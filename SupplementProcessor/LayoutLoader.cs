using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace SupplementProcessor
{
    [Obsolete]
    public class LayoutLoader
    {
        public delegate void LayoutLoaderEventHandler(LayoutLoader sender, LayoutProperties prop);
        public event LayoutLoaderEventHandler OnLayoutLoaded;

        private static bool m_ISLoading;

        public LayoutProperties LayoutProperties { set; get; }

        public LayoutLoader()
        { }

        public LayoutLoader(string layoutName, string layoutConfigFile)
        {
            LoadLayoutAsync(layoutName, layoutConfigFile);
        }

        public LayoutProperties LoadLayout(string layoutName, string layoutConfigFile)
        {
            LayoutProperties = null;
            var filename = Path.Combine(Directory.GetCurrentDirectory(), Properties.Settings.Default.LayoutDir, layoutName, layoutConfigFile);
            using (Stream stream = new FileStream(filename, FileMode.Open))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(LayoutProperties));
                try
                {
                    LayoutProperties = (LayoutProperties)serializer.Deserialize(stream);
                    if (OnLayoutLoaded != null)
                        OnLayoutLoaded(this, LayoutProperties);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            m_ISLoading = false;
            return LayoutProperties;
        }

        public void LoadLayoutAsync(string layoutName, string layoutSide)
        {
            if (m_ISLoading)
                return;

            m_ISLoading = true;

            Task.Factory.StartNew(new Action(() =>
            {
                LoadLayout(layoutName, layoutSide);
            }));
        }
    }
}
