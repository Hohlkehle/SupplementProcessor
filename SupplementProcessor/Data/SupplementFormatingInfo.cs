using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupplementProcessor.Data
{
    public struct SupplementFormatingInfo
    {
        private bool m_AssessmentByWordsOnly;
        private bool m_AssessmentsOnLastLine;
        private bool m_SkipEmplyLines;

        public bool AssessmentByWordsOnly
        {
            get { return m_AssessmentByWordsOnly; }
            set { m_AssessmentByWordsOnly = value; }
        }
      
        public bool AssessmentsOnLastLine
        {
            get { return m_AssessmentsOnLastLine; }
            set { m_AssessmentsOnLastLine = value; }
        }

        public bool SkipEmplyLines
        {
            get { return m_SkipEmplyLines; }
            set { m_SkipEmplyLines = value; }
        }

        public SupplementFormatingInfo(bool? skipEmplyLines, bool? assessmentsOnLastLine, bool? assessmentByWordsOnly)
            : this((bool)skipEmplyLines, (bool)assessmentsOnLastLine, (bool)assessmentByWordsOnly)
        { }

        public SupplementFormatingInfo(bool skipEmplyLines, bool assessmentsOnLastLine, bool assessmentByWordsOnly)
        {
            m_AssessmentByWordsOnly = assessmentByWordsOnly;
            m_AssessmentsOnLastLine = assessmentsOnLastLine;
            m_SkipEmplyLines = skipEmplyLines;
        }
       
    }
}
