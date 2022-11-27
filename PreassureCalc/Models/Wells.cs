namespace PreassureCalc.Models
{
    public class Wells 
    {
        

        public int id { get; set; }
        public string numberWell { get; set; }
        public float depth { get; set; }
        public float preassure { get; set; }
        public float calculatedDepth { get; set; }
        public float density { get; set; }

        
    }
}
