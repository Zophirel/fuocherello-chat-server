using System.ComponentModel.DataAnnotations;

namespace SignalRChatServer.Models;
public class UserConnection{

    public UserConnection(){
        Id = Guid.NewGuid();
    }
    public UserConnection(string ConnectionId, string UserId)
    {
        Id = Guid.NewGuid();
        this.ConnectionId = ConnectionId;
        this.UserId = UserId;
    }

    [Key]
    public Guid Id {get; set;}
    public string? ConnectionId {get; set;}
    public string? UserId {get; set;}

}