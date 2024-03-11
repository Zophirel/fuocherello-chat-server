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


    public UserMessage FromJson(string json){
        try{
            Console.WriteLine(json);
            JObject message = JObject.Parse(json);
            if(message["chat_id"] is null || message["chat_id"]!.ToString() == "" || message["chat_id"]!.ToString() == "null"){
                Id = Guid.NewGuid();
                ChatId = null;
            }else{
                ChatId = Guid.Parse(message["chat_id"]!.ToString());
            }

            ProdId = Guid.Parse(message["prod_id"]!.ToString());
            SenderId = message["from"]!.ToString();
            Message = message["message"]!.ToString();
            SentAt = ulong.Parse(message["sent_at"]!.ToString());
            Delivered = false;
            return this;
        }catch (Exception e)
        {
            Console.WriteLine(e);
            return this;
        }

    }

    public string ToAppMessage(string receiverId){
        string begin = "{";
        string end = "}";
        string json = $""" "chat_id" : "{ChatId}", "prod_id" : "{ProdId}", "sender_id" : "{SenderId}", "receiver_id" : "{receiverId}", "message" : "{Message}", "sent_at" : "{SentAt}" """;
        json = begin + json.Substring(1, json.Length-1) + end;
        return json;
    }

    public string ObjectToAppMessage(UserMessage m, string receiver){
        string begin = "{";
        string end = "}";
        string json = $""" "id":"{m.Id}", "chat_id" : "{m.ChatId}", "prod_id" : "{m.ProdId}", "from" : "{m.SenderId}", "to" : "{receiver}", "message" : "{Message}", "sent_at" : "{SentAt}" """;
        json = begin + json.Substring(1, json.Length-1) + end;
        return json;
    }

}