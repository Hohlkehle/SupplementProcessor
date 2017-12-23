using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupplementProcessor.Data
{
    public class StudentInfo
    {
        public int OrderNumber { set; get; }
        public string FullName { get { return FirstName + " " + SecondName; } }
        public string FirstName { set; get; }
        public string SecondName { set; get; }
        public int Sex { set; get; }
        public string DiplomaSerial { set; get; }
        public string DiplomaNumber { set; get; }
        public string RegisterNumber { set; get; }

        public List<string> Assessments = new List<string>();
        public List<string> RawAssessments = new List<string>();

        public string this[int index, bool raw = false]
        {
            get
            {
                try
                {
                    return raw ?
                        RawAssessments[index] :
                        Helper.FormatAssessments(RawAssessments[index].ToString(), Sex);
                }
                catch (InvalidOperationException) { return ""; }
                catch (IndexOutOfRangeException) { return ""; }
            }
            set
            {
                try
                {
                    if (raw)
                        RawAssessments[index] = value;
                    else
                        Assessments[index] = value;
                }
                catch (InvalidOperationException) { }
                catch (IndexOutOfRangeException) { }
            }
        }

        public StudentInfo()
        {

        }

        public StudentInfo(List<string> data)
        {
            try
            {
                int order = -1;
                int.TryParse(data[0].ToString(), out order);
                OrderNumber = order;

                var fullName = (data[1]).Split(' ');
                if (fullName.Length > 2)
                {
                    FirstName = fullName[0];
                    //SecondName = fullName[1] + " " + fullName[2];
                    for (var i = 1; i < fullName.Length; i++ )
                    {
                        SecondName += fullName[i];
                        if (i != fullName.Length - 1)
                            SecondName += " ";
                    }
                }
                else if (fullName.Length == 1)
                {
                    FirstName = fullName[0];
                    SecondName = "";
                }
                else
                {
                    FirstName = "Error";
                    SecondName = "Unable to parse name";
                }

                Sex = data[2].ToString().ToLower() == "ч" ? 0 : 1;
                DiplomaSerial = data[3];
                DiplomaNumber = data[4];
                RegisterNumber = data[5];
            }
            catch (IndexOutOfRangeException) { }
            catch (ArgumentOutOfRangeException) { }

            for (var i = Properties.Settings.Default.DISCIPLINES_START_INDEX; i < data.Count; i++)
            {
                RawAssessments.Add(data[i]);
                Assessments.Add(Helper.FormatAssessments(data[i].ToString(), Sex));
            }
        }

        [Obsolete]
        public StudentInfo(DataRow dataRow)
        {
            var fullName = (dataRow[1].ToString()).Split(' ');
            FirstName = fullName[0];
            SecondName = fullName[1] + " " + fullName[2];

            Sex = int.Parse(dataRow[2].ToString());
            DiplomaSerial = dataRow[3].ToString();
            DiplomaNumber = dataRow[4].ToString();

            for (var i = 5; i < dataRow.ItemArray.Length; i++)
            {
                Assessments.Add(dataRow[i].ToString());
            }
        }

        public string GetValue(string column)
        {
            var result = "";

            if (column == "Нет")
            {
                result = "";
            }
            else if (column == "Серия атестата")
            {
                result = DiplomaSerial;
            }
            else if (column == "№ атестата")
            {
                result = DiplomaNumber;
            }
            else if (column == "Фамилия")
            {
                result = FirstName;
            }
            else if (column == "Имя Отчество")
            {
                result = SecondName;
            }
            else if (column == "закінчів/ла")
            {
                result = Helper.assessmentsRecived[0][Sex];
            }
            else if (column == "пройшов/ла")
            {
                result = Helper.assessmentsRecived[1][Sex];
            }
            else if (column == "засвоїв/ла")
            {
                result = Helper.assessmentsRecived[0][Sex];
            }
            else if (column == "Регистрационный номер")
            {
                result = RegisterNumber;
            }
            else if (column == "Серия диплома")
            {
                result = DiplomaSerial;
            }
            else if (column == "№ диплома")
            {
                result = DiplomaNumber;
            }
            else if (column == "навчався/лася")
            {
                result = Helper.educationRecived[(int)MainWindow.instance.LayoutProperties.LayoutType][Sex];
            }
            else if (column == "одержав/ла")
            {
                result = Helper.assessmentsRecived[(int)MainWindow.instance.LayoutProperties.LayoutType][Sex];
            }


            return result;
        }

        public override string ToString()
        {
            if (FullName == "")
                return base.ToString();

            return string.Format("{0}. {1} {2}{3}", OrderNumber, FullName, DiplomaSerial, DiplomaNumber);


            //var spl = SecondName.Split(' ');
            //var initials = new string[2] { "", "" };

            //if (spl.Length == 2)
            //{
            //    initials[0] = spl[0];
            //    initials[1] = spl[1];
            //}
            //else if (spl.Length == 1)
            //{
            //    initials[0] = spl[0];
            //}

            //return string.Format("{0} {1}.{2}. {3}{4}", FirstName, char.ToUpper(initials[0][0]), char.ToUpper(initials[1][0]), DiplomaSerial, DiplomaNumber);
        }

        public override int GetHashCode()
        {
            return OrderNumber.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return this == ((StudentInfo)obj);
        }
        public static bool operator !=(StudentInfo a, StudentInfo b)
        {
            return !(a == b);
        }

        public static bool operator ==(StudentInfo a, StudentInfo b)
        {
            if (ReferenceEquals(a, b))
                return true;


            if (a is StudentInfo && ((StudentInfo)a).FullName != "")
            {
                if (b is StudentInfo && ((StudentInfo)b).FullName != "")
                {
                    return ((StudentInfo)a).FullName == ((StudentInfo)b).FullName && ((StudentInfo)a).Sex == ((StudentInfo)b).Sex;
                }
                else
                    return false;
            }
            else
                return false;
        }

        internal bool Format(SupplementFormatingInfo supplementFormatingInfo)
        {
            Assessments.Clear();

            for (var i = 0; i < RawAssessments.Count; i++)
            {
                Assessments.Add(Helper.FormatAssessments(RawAssessments[i].ToString(), Sex, supplementFormatingInfo));
            }

            return true;
        }
    }
}
