using System;

namespace BlazingPoints.ViewModels
{
    public class RollUpBaseVm
    {
        public DateTime AsOf { get; set; }
        public string AsOfTime { get { return AsOf.ToShortTimeString();  } }
        public string DayOfSprint { get { return AsOf.ToString("d MMM"); } } 
    }
}  