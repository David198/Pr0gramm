using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pr0gramm.Helpers
{
    public static class DateTimeUtlis
    {
        public static string MakeCreatedString(DateTime Created)
        {
            if ((DateTime.Now - Created).Minutes < 1)
                return "JustNow".GetLocalized();
            if ((DateTime.Now - Created).Hours < 1)
            {
                var minutes = (DateTime.Now - Created).Minutes;
                var returnString = "Before".GetLocalized() + " " + minutes + " ";
                if (minutes > 1) return returnString + "Minutes".GetLocalized();
                return returnString + "Minute".GetLocalized();
            }
            if ((DateTime.Now - Created).Days < 1)
            {
                var hours = (DateTime.Now - Created).Hours;
                var returnString = "Before".GetLocalized() + " " + hours + " ";
                if (hours > 1) return returnString + "Hours".GetLocalized();
                return returnString + "Hour".GetLocalized();
            }
            if ((DateTime.Now - Created).Days > 1)
            {
                var days = (DateTime.Now - Created).Days;
                var returnString = "Before".GetLocalized() + " " + days + " ";
                if (days > 1) return returnString + "Days".GetLocalized();
                return returnString + "Day".GetLocalized();
            }
            return "";
        }
    }
}
