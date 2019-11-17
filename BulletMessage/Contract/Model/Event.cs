using System;
namespace BulletMessage.Contract.Model
{
    public class Event
    {
        public string ConnectionId { get; set; }
        public string EventId { get; set; }
        public Event()
        {
        }
        public Event(string connectionid,string eventid)
        {
            this.ConnectionId = connectionid;
            this.EventId = connectionid;
        }
    }
}
