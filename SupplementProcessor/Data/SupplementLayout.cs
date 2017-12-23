﻿using LayoutFileSystem;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Serialization;

namespace SupplementProcessor.Data
{
    public class SupplementLayout
    {
        public static event EventHandler OnSupplementLayoutLoaded;

        private Layout m_Layout;
        private string m_FileName;

        public LayoutProperties FrontSideLayout { get; set; }

        public LayoutProperties RearSideLayout { get; set; }

        public bool m_IsLoading { get; set; }

        public bool IsChanged { get; set; }

        public Exception LastException { get; set; }

        public bool IsLoaded { get { return FrontSideLayout != null && RearSideLayout != null && LastException == null; } }

        public SupplementLayout()
        { }

        public bool Create(string fileName, string layoutName, LayoutType layoutType)
        {
            m_FileName = fileName;
            m_Layout = new Layout();

            FrontSideLayout = new LayoutProperties()
            {
                Name = layoutName,
                LayoutType = layoutType,
                Size = new System.Windows.Point(21, 14.8),
                BackgroundImage = LayoutFileReader.ImageToByte(Properties.Resources.DefaultBackground)
            };

            RearSideLayout = new LayoutProperties()
            {
                Name = layoutName,
                LayoutType = layoutType,
                Size = new System.Windows.Point(21, 14.8),
                BackgroundImage = LayoutFileReader.ImageToByte(Properties.Resources.DefaultBackground)
            };

            try
            {
                Save();

                return true;
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.TraceError(e.ToString());
                LastException = e;
            }

            return false;
        }

        public void LoadFileAsync(string fileName)
        {
            m_FileName = fileName;

            if (m_IsLoading)
                return;

            m_IsLoading = true;

            Task.Factory.StartNew(new Action(() =>
            {
                Load();
            }));
        }

        public void LoadFile(string fileName)
        {
            m_FileName = fileName;

            Load();
        }

        private void Load()
        {
            try
            {
                m_Layout = new Layout(m_FileName);
            }
            catch (FileLoadException )
            {
                MessageBox.Show("Layout file "+m_FileName+" cannot be loaded!", "Supplement Processor", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                m_IsLoading = false;
                return;
            }

            FrontSideLayout = ReadLayoutXml(LayoutFileReader.GetString(m_Layout.Data1));
            RearSideLayout = ReadLayoutXml(LayoutFileReader.GetString(m_Layout.Data2));

            FrontSideLayout.BackgroundImage = m_Layout.Data3;
            RearSideLayout.BackgroundImage = m_Layout.Data4;

            m_IsLoading = false;

            if (OnSupplementLayoutLoaded != null)
                OnSupplementLayoutLoaded(this, EventArgs.Empty);
        }

        public LayoutProperties ReadLayoutXml(string xml)
        {
            LayoutProperties layoutProperties = null;

            using (var reader = new StringReader(xml))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(LayoutProperties));
                    layoutProperties = (LayoutProperties)serializer.Deserialize(reader);
                }
                catch (Exception ex)
                {
                    LastException = ex;
                    MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            return layoutProperties;
        }

        public LayoutProperties GetProperties(LayoutSide side)
        {
            if (side == LayoutSide.Front)
                return FrontSideLayout;

            return RearSideLayout;
        }

        public void Save()
        {
            Update();

            m_Layout.Save(m_FileName);

            IsChanged = false;
        }

        string SerializeToString(LayoutProperties prop)
        {
            prop.Check();

            var xml = "";
            using (var writer = new StringWriter())
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(LayoutProperties));
                    serializer.Serialize(writer, prop);
                    xml = writer.GetStringBuilder().ToString();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "SupplementLayout.SerializeToString()", MessageBoxButton.OK);
                }
            }
            return xml;
        }

        internal void Update()
        {
            if (FrontSideLayout.m_CaptionInfo.Count == RearSideLayout.m_CaptionInfo.Count && (FrontSideLayout.m_CaptionInfo.Count != 0 || RearSideLayout.m_CaptionInfo.Count != 0))
            {
                if (MessageBox.Show("Maybe error ocurred! Terminate programm?", "SupplementLayout.Update()", MessageBoxButton.YesNo, MessageBoxImage.Asterisk) == MessageBoxResult.Yes)
                {
                    MainWindow.instance.Close();
                    //throw new InvalidOperationException();
                }
            }
            m_Layout.Data1 = LayoutFileReader.GetBytes(SerializeToString(FrontSideLayout));
            m_Layout.Data2 = LayoutFileReader.GetBytes(SerializeToString(RearSideLayout));

            if (!ReferenceEquals(m_Layout.Data3, FrontSideLayout.BackgroundImage))
                m_Layout.Data3 = FrontSideLayout.BackgroundImage;

            if (!ReferenceEquals(m_Layout.Data4, RearSideLayout.BackgroundImage))
                m_Layout.Data4 = RearSideLayout.BackgroundImage;

            IsChanged = true;
        }
    }
}
