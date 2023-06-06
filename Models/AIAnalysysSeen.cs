using System;
namespace MessengerBackend.Models
{
	public class AIAnalysysSeen
    {
        public int Id { get; set; } 
        public string UserId { get; set; }
        public int AnalysysId { get; set; }
        public AIAnalysys Analysys { get; set; }
        public bool WasSeen { get; set; }
    }
}

