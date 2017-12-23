using System;
using System.IO;
using System.IO.Packaging;
using System.Printing;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Xps.Packaging;

namespace SupplementProcessor
{
    public class XpsDocumentPainter
    {
        public static FixedDocumentSequence PaintDrawingVisual(DrawingVisual drawingVisual, PageMediaSize pageMediaSize)
        {
            FixedDocumentSequence document = null;
            using (var xpsStream = new MemoryStream())
            {
                using (var package = Package.Open(xpsStream, FileMode.Create, FileAccess.ReadWrite))
                {
                    var packageUriString = "memorystream://data.xps";
                    var packageUri = new Uri(packageUriString);

                    PackageStore.AddPackage(packageUri, package);

                    var xpsDocument = new XpsDocument(package, CompressionOption.Maximum, packageUriString);
                    var writer = XpsDocument.CreateXpsDocumentWriter(xpsDocument);
                    var printTicket = new PrintTicket();
                    printTicket.PageMediaSize = pageMediaSize;

                    writer.Write(drawingVisual, printTicket);

                    document = xpsDocument.GetFixedDocumentSequence();
                    xpsDocument.Close();
                    PackageStore.RemovePackage(packageUri);
                }
            }
            return document;
        }
    }
}
