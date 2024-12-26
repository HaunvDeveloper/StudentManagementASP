namespace StudentManagementASP.ViewModels
{
    public class ChartViewModel
    {
        public PieChartData pieChartData { get; set; } = new PieChartData();
        public BlockChartData blockChartData { get; set; } = new BlockChartData();
    }

    public class PieChartData
    {
        public int Attended {  get; set; }  

        public int Absent { get; set; }

        public int Late { get; set; }
    }

    public class BlockChartData
    {
        public List<BlockChartDetail> data { get; set; } = new List<BlockChartDetail>();
    }

    public class BlockChartDetail
    {
        public string Subject { get; set; }

        public int Attended { get; set; }

        public int Absent { get; set; }

        public int Late { get; set; }
    }
}
