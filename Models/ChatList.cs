using System.ComponentModel.DataAnnotations;

namespace SignalRChatServer.Models;
public class ChatList{

    public ChatList(){
        Id = Guid.NewGuid();
    }
    public ChatList(Guid prodId, string buyerId, string sellerId)
    {
        
        Id = Guid.NewGuid();
        ProdId = prodId;
        BuyerId = buyerId;
        SellerId = sellerId;
    }

    [Key]
    public Guid Id {get; set;}
    public Guid ProdId {get; set;}
    public string? BuyerId {get; set;}
    public string? SellerId {get; set;}

}