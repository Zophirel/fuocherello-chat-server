using Newtonsoft.Json.Linq;

namespace SignalRChatServer.Models;
public class UserMessage{
    public  Guid? Id {get; set;}
    public  Guid? ChatId {get; set;}
    public  Guid? ProdId {get; set;}
    public string? SenderId {get; set;}
    public string? Message {get; set;}
    public  ulong? SentAt {get; set;}
    public  bool? Delivered {get; set;}

    public UserMessage(){}


    public UserMessage fromJson(string json){
        try{
            Guid? chat_id;
            Console.WriteLine(json);
            JObject message = JObject.Parse(json);
            if(message["chatId"] is null || message["chatId"]!.ToString() == "" || message["chatId"]!.ToString() == "null"){
                Id = Guid.NewGuid();
            }else{
                Console.WriteLine(message["chatId"]!.ToString());
                chat_id = Guid.Parse(message["chatId"]!.ToString());
                if(chat_id != null){
                    ChatId = chat_id;
                } 
            }
            ProdId = Guid.Parse(message["prodId"]!.ToString());
            SenderId = message["from"]!.ToString();
            Message = message["message"]!.ToString();
            SentAt = ulong.Parse(message["sentAt"]!.ToString());
            Delivered = false;
            return this;
        }catch (Exception e)
        {
            Console.WriteLine(e);
            return this;
        }

    }

    public string toAppMessage(string receiverId){
        string begin = "{";
        string end = "}";
        string json = $""" "chat_id" : "{ChatId}", "prod_id" : "{ProdId}", "sender_id" : "{SenderId}", "receiver_id" : "{receiverId}", "message" : "{Message}", "sent_at" : "{SentAt}" """;
        json = begin + json.Substring(1, json.Length-1) + end;
        return json;
    }

    public string ObjectToAppMessage(UserMessage m, string receiver){
        string begin = "{";
        string end = "}";
        string json = $""" "id":"{m.Id}", "chatId" : "{m.ChatId}", "prodId" : "{m.ProdId}", "from" : "{m.SenderId}", "to" : "{receiver}", "message" : "{Message}", "sentAt" : "{SentAt}" """;
        json = begin + json.Substring(1, json.Length-1) + end;
        return json;
    }

}