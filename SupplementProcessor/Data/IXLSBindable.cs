using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupplementProcessor.Data
{
    interface IXLSBindable
    {
        string XlsColumn { set; get; }
        void SetText(string text);
    }
}
