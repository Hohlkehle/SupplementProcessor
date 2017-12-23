using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupplementProcessor
{
    public enum LayoutType
    {
        ColledgeAttachment = 0,
        School9Attachment = 1,
        School11Attachment = 2,
        UniversityAttachment = 3
    }

    public class LayoutTypeHelper
    {
        public const string TYPE_1 = "Диплом квалифицированного работника";
        public const string TYPE_2 = "Аттестат о базовом (9-кл) среднем образовании";
        public const string TYPE_3 = "Аттестат о полном (11-кл) среднем образовании";
        public const string TYPE_4 = "Диплом бакалавра";

        public static LayoutType ToLayoutType(string text)
        {
            LayoutType type = LayoutType.ColledgeAttachment;

            if (text == TYPE_1)
            {
                type = LayoutType.ColledgeAttachment;
            }
            else if (text == TYPE_2)
            {
                type = LayoutType.School9Attachment;
            }
            else if (text == TYPE_3)
            {
                type = LayoutType.School11Attachment;
            }
            else if (text == TYPE_4)
            {
                type = LayoutType.UniversityAttachment;
            }
            return type;
        }

        public static string ToString(LayoutType type)
        {
            switch (type)
            {
                case LayoutType.ColledgeAttachment:
                    return TYPE_1;
                case LayoutType.School9Attachment:
                    return TYPE_2;
                case LayoutType.School11Attachment:
                    return TYPE_3;
                case LayoutType.UniversityAttachment:
                    return TYPE_4;
                default:
                    return TYPE_1;
            }
        }
    }
}
