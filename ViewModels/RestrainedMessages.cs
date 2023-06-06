using System;
using MessengerBackend.Models;
namespace MessengerBackend.ViewModels
{
	public class RestrainedMessages
	{
		public User User { get; set; }
		public string Reason { get; set; }
		public int AnalysysID { get; set; }
		public string Messege { get; set; }

		public RestrainedMessages()
		{
		}
	}
}

