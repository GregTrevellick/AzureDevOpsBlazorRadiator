using BlazingPoints.Api.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BlazingPoints.ViewModels
{
    public class SprintProgressVm
    {
        private IEnumerable<BurndownFigureRollUpVm> _burndownFigureRollUpVms;
        public IEnumerable<BurndownFigureRollUpVm> BurndownFigureRollUpVms
        {
            get
            {
                if (_burndownFigureRollUpVms == null)
                {
                    _burndownFigureRollUpVms = GetBurndownFigureRollUpVms();
                }
                return _burndownFigureRollUpVms;
            }
        }
        public string DebugString { get; set; }
        public float idealPointsBurntPerDay { get { return PointsOnDayOne / lengthOfSprint; } }
        public float PointsOnDayOne { get { return WorkItemRollUpVms.First().TotalPoints.HasValue ? WorkItemRollUpVms.First().TotalPoints.Value : 0; } }
        private int lengthOfSprint { get { return WorkItemRollUpVms.Count(); } }//10
        public string IterationNumber { get; set; }
        private IEnumerable<DateTime> _sprintDates 
        { 
            get 
            {
                return WorkItemVms.Select(x=>x.AsOf).Distinct();          
            } 
        }
        public DateTime SprintEnd { get; set; } 
        public DateTime SprintStart { get; set; } 
        public IList<WorkItemVm> WorkItemVms { get; set; }
        private IEnumerable<WorkItemRollUpVm> _workItemRollUpVms;
        public IEnumerable<WorkItemRollUpVm> WorkItemRollUpVms 
        {
            get
            {
                if (_workItemRollUpVms == null)
                {
                    _workItemRollUpVms = GetWorkItemRollUpVms();
                }
                return _workItemRollUpVms;
            }
        }

        internal IEnumerable<BurndownFigureRollUpVm> GetBurndownFigureRollUpVms()
        {
            var result = new List<BurndownFigureRollUpVm>();

            var i = 1;
            float idealPointsRemainingThisDay;

            foreach (var sprintDayDate in _sprintDates)
            {
                if (i == 1)
                {
                    idealPointsRemainingThisDay = PointsOnDayOne;
                }
                else
                {
                    idealPointsRemainingThisDay = PointsOnDayOne - (idealPointsBurntPerDay * (i - 1));
                }

                var workItemVm = GetWorkItemVmByDate(sprintDayDate);

                if (IsWorkItemVmPopulated(workItemVm))
                {
                    float? donePoints = null;

                    var sprintDateReached = IsSprintDateReached(sprintDayDate);

                    if (sprintDateReached)
                    {
                        donePoints = GetDonePoints(workItemVm);
                    }

                    result.Add(new BurndownFigureRollUpVm
                    {
                        ActualDone = donePoints,
                        AsOf = sprintDayDate,
                        IdealRemaining = idealPointsRemainingThisDay,
                        PointsOnDayOne = PointsOnDayOne
                    });
                }

                i++;
            }

            return result;
        }

        internal IEnumerable<WorkItemRollUpVm> GetWorkItemRollUpVms()
        {
            var result = new List<WorkItemRollUpVm>();

            foreach (var sprintDayDate in _sprintDates)
            {
                var workItemVm = GetWorkItemVmByDate(sprintDayDate);

                if (IsWorkItemVmPopulated(workItemVm))
                {
                    float? totalPoints = 0;
                    int? workItemsCount = 0;

                    var sprintDateReached = IsSprintDateReached(sprintDayDate);

                    if (sprintDateReached)
                    {
                        totalPoints = workItemVm.Select(x => x.Effort).Sum();
                        workItemsCount = workItemVm.Count();
                    }

                    result.Add(new WorkItemRollUpVm
                    {
                        AsOf = sprintDayDate,
                        TotalPoints = totalPoints,
                        WorkItemsCount = workItemsCount,
                    });
                }
            }

            return result;
        }

        private static float? GetDonePoints(IEnumerable<WorkItemVm> workItemVm)
        {
            var result = workItemVm.Where(x => x.State?.ToLower() == "closed" || x.State?.ToLower() == "done").Sum(x => x.Effort);
            return result;
        }

        private static bool IsSprintDateReached(DateTime sprintDayDate)
        {
            return sprintDayDate <= DateTime.Now;
        }

        private IEnumerable<WorkItemVm> GetWorkItemVmByDate(DateTime sprintDayDate)
        {
            return WorkItemVms.Where(x => x.AsOf.Date == sprintDayDate.Date);
        }

        private static bool IsWorkItemVmPopulated(IEnumerable<WorkItemVm> workItemVm)
        {
            return workItemVm != null && workItemVm.Count() > 0;
        }

        public UiDataObject UiDataObject 
        {
            get 
            {
                var iterationDetails = new Iterationdetails
                {
                    Iteration = IterationNumber, 
                    Start = SprintStart.ToShortDateString(), 
                    End = SprintEnd.ToShortDateString()
                };
                
                var workItemDatas = new List<Workitemdata>();
                
                foreach (var workItemVm in WorkItemVms)
                {
                    var workItemUiData = new Workitemdata
                    {
                        Date = workItemVm.AsOf.ToShortDateString() + " " + workItemVm.AsOf.ToShortTimeString(),
                        Id = workItemVm.Id.HasValue ? workItemVm.Id.Value : 0,
                        Points = workItemVm.Effort.HasValue ? workItemVm.Effort.Value : 0,
                        Status = workItemVm.State,
                    };
                    
                    workItemDatas.Add(workItemUiData);
                }         

                return new UiDataObject
                {
                    IterationDetails = iterationDetails,
                    WorkItemData = workItemDatas.ToArray()
                };
            }
        }
    }
}
