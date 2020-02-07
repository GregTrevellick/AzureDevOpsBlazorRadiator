namespace BlazingPoints.Api.DTOs
{
    public class UiDataObject
    {
        public Iterationdetails IterationDetails { get; set; }
        public Workitemdata[] WorkItemData { get; set; }
    }

    public class Iterationdetails
    {
        public string Iteration { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
    }

    public class Workitemdata
    {
        public string Date { get; set; }
        public int Id { get; set; }
        public float Points { get; set; }
        public string Status { get; set; }
    }

}
