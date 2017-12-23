using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupplementProcessor.Data
{
    public class RowDataInfo
    {
        public string this[string field]
        {
            get { return Fields[field]; }
            set { Fields[field] = value; }
        }
        public Dictionary<string, string> Fields
        {
            get;
            set;
        }
        public Dictionary<string,string> Table
        {
            get;
            set;
        }

        public RowDataInfo()
        {
            Fields = new Dictionary<string, string>();
            Table = new Dictionary<string, string>();
        }

        public override string ToString()
        {
            if (Fields != null && Fields.Count > 3)
                return Fields[Fields.Keys.ElementAt(0)] + ". " + Fields[Fields.Keys.ElementAt(1)] + " " + Fields[Fields.Keys.ElementAt(2)];
            return base.ToString();
        }
    }
}
