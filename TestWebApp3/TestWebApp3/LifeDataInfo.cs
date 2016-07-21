using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TestWebApp3
{
    public class LifeDataInfo
    {
        public int NumPeopleInSample { get; set; }
        public int MaxLifeYearBinValue { get; set; }
        public IEnumerable<int> YearsWithHighestLifeCountList { get; set; }
        public IEnumerable<string> YearList { get; set; }
        public IEnumerable<int> NumLivesList { get; set; }
    }
}