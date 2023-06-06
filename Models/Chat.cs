using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MessengerBackend.Models
{
    public class Chat
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        public int LastProcessedMessageId { get; set; }
        public string Name { get; set; } // Nullable, used for group chat names
        public bool IsGroupChat { get; set; }
        public ICollection<ChatParticipant> ChatParticipants { get; set; }
        public ICollection<Message> Messages { get; set; } // Add this line

    }
}

