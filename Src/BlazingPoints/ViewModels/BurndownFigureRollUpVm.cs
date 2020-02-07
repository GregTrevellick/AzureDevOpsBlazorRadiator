namespace BlazingPoints.ViewModels
{
    public class BurndownFigureRollUpVm : RollUpBaseVm  
    {
        internal float? ActualDone { get; set; }
        public float? ActualRemaining 
        { 
            get 
            {
                var actualRemaining = PointsOnDayOne - ActualDone;
                return actualRemaining >= 0 ? actualRemaining : 0;
            }
        }
        public float IdealRemaining { get; set; }
        internal float PointsOnDayOne { get; set; }
    }
}
