using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupplementProcessor.Data
{
    public class PrintWizarDataSource
    {
        public string CurrentLayout { set; get; }

        public int SelectedIndex { get; set; }

        public int MaxIndex { get; set; }

        public List<StudentInfo> StudentInfos { set; get; }
        public List<string> DisciplineLabels { set; get; }
    }
}
