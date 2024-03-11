using System.IdentityModel.Tokens.Jwt;
using SignalRChatServer.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using SignalRChatServer.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using SignalRChatServer.Singleton.JwtManager;
namespace SignalRChatServer.Hubs
{
    public class ChatHub : Hub
    {  
        private readonly ChatDbContext _context;
        private static readonly Dictionary<string, string> _pingMessageIds = new();
        private static readonly IConfigurationRoot _configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        private readonly IJwtManager _manager;
        public ChatHub(ChatDbContext context, IJwtManager manager){
            _context = context;
            _manager = manager;
            
        }        
        public string GetConnectionId() => Context.ConnectionId;
    
        public async Task BroadcastToConnection(string data, string connectionId)    
            => await Clients.Client(connectionId).SendAsync("broadcasttoclient", data);

        public override Task OnDisconnectedAsync(Exception? exception) {
            System.Diagnostics.Trace.WriteLine($"{Context.ConnectionId} - reconnected");
            System.Diagnostics.Trace.WriteLine($"error: {exception}");
            return Task.CompletedTask; 
        }

        public bool PingSuccess(string token, string code){
            Console.WriteLine("Ping Success");
            if( _manager.ValidateAccessToken(token) == TokenStatus.Valid){
                if(_pingMessageIds.Remove(code)){
                   return true;
                } 
            }
            return false;
        } 

        private async Task<string> Ping(string sub){
            Console.WriteLine("Pinged!");
            var userToPing = _context.ChatServerConnection.FirstOrDefault(conn => conn.UserId == sub);
            if(userToPing != null){
                Console.WriteLine($"utente da contattare {userToPing.UserId}");
                string randomPingCode = Guid.NewGuid().ToString();
                _pingMessageIds.Add(randomPingCode, sub);
                await Clients.Client(userToPing.ConnectionId!).SendAsync("ping", randomPingCode);
                return randomPingCode;
            }
            return "";
        }

        private async Task NotifySenderOfNewConversation(string userConn, string sellerId, Guid? chatId, Product prod)
        {            

            Dictionary<string, string> data = new()
            {
                { "id", chatId.ToString()! },
                { "prod_id", prod.Id.ToString()! },
                { "prod_name", prod.Title! },
                { "contact_id", sellerId! },
                { "not_read_message", "0" },
                { "thumbnail", prod.ProductImages?.FirstOrDefault() ?? ""}               
            };

            string contactForSender = JsonSerializer.Serialize(data);
            await Clients.Client(userConn).SendAsync("sendchatinfo", contactForSender);
        }

        private async Task NotifyReceiverOfNewConversation(string userConn, string buyerId, Guid? chatId, Product prod){
            User? buyer = _context.Users.SingleOrDefault(user => user.HashedId  == buyerId);
            if(buyer != null){

                Dictionary<string, string> contactData = new(2)
                {
                    { "contact_id", buyer.HashedId! },
                    { "contact_name", buyer.Name! }
                };

                DateTime currentTime = DateTime.UtcNow;
                long sent_at = ((DateTimeOffset)currentTime).ToUnixTimeSeconds();
                Dictionary<string, string> appChatTileData = new(5)
                {
                    { "id", chatId.ToString()! },
                    { "prod_id", prod.Id.ToString()! },
                    { "prod_name", prod.Title! },
                    { "contact_id", buyer.HashedId! },
                    { "not_read_message", "0" },
                    { "thumbnail", prod.ProductImages?.FirstOrDefault() ?? "" }
                };
                
                List<Dictionary<string, string>> data = new(2)
                {
                    contactData,
                    appChatTileData
                };

                string json = JsonSerializer.Serialize(data);

                await Clients.Client(userConn).SendAsync("notifyuserofnewconversation", json);
            }
        }
  
        public async Task OnDisconnet(){
            var user = _context.ChatServerConnection.First(user => user.ConnectionId == GetConnectionId());
            _context.ChatServerConnection.Remove(user);
            await _context.SaveChangesAsync();
        }
        
        private static async Task<bool> CheckForPingValue(string pingCode){
            Task delay = Task.Delay(1000);
            for(int i = 0; i < 5; i++){
                if(_pingMessageIds.Keys.Contains(pingCode)){
                    await delay;
                    Console.WriteLine("time waited");
                }else{
                    return false;
                }  
            }  
            _pingMessageIds.Remove(pingCode);
            return true;
        }

        private async Task SendFirstMessageToOfflineUser(UserMessage message, string receiver, Product prod, ChatList chat){
            Console.WriteLine("pinged user offline");

            //send receiver info only to sender because receiver is currently offline
            await NotifySenderOfNewConversation(GetConnectionId(), receiver, chat!.Id, prod);
            _context.ChatList.Add(chat!);
            await _context.SaveChangesAsync();
            
            //if the chat wasn't created before then the first 
            //message received dose not contain any chat id
            //so i need to assign it here
            message.ChatId = chat.Id;
            _context.Message.Add(message);
            await _context.SaveChangesAsync();
        }

        private async Task SendFirstMessageToOnlineUser(string dataToTransmit, Product prod, string sender, string receiver, UserMessage message, ChatList chat){
            Console.WriteLine("pinged user online");
            string? receiverConnecectionId = _context.ChatServerConnection.SingleOrDefault(conn => conn.UserId == receiver)!.ConnectionId;
            
            //exchange contacts
            await NotifySenderOfNewConversation(GetConnectionId(), receiver, message.ChatId, prod);
            await NotifyReceiverOfNewConversation(receiverConnecectionId!, sender, message.ChatId, prod);
            //wait for the client to register contact and chat data
            await Task.Delay(2000);    
            //send message to client
            await BroadcastToConnection(dataToTransmit, receiverConnecectionId!); 
            
            //save newly created data and first message data to db
            _context.ChatList.Add(chat!);
            await _context.SaveChangesAsync();
            
            message.Delivered = true;

            //if the chat hasn't been created yet the first 
            //message received dose not contain any chat id
            //so it will be assigned here
            message.ChatId = chat.Id;
            _context.Message.Add(message);
            await _context.SaveChangesAsync();
        }

        private async Task SendMessageToOnlineUser(string dataToTransmit, UserMessage message, string receiverConnecectionId){
            await BroadcastToConnection(dataToTransmit, receiverConnecectionId);
            message.Delivered = true;
            _context.Message.Add(message);
            await _context.SaveChangesAsync();
        }
        private async Task SendMessageToOfflineUser(UserMessage message){
            //deliverd is false by default
            _context.Message.Add(message);
            await _context.SaveChangesAsync();
        }

        private string PrepareMessage(string senderId, string jsonMessage)
        {
            string? sender_name = _context.Users.SingleOrDefault(user => user.HashedId == senderId)!.Name;
            string senderNameJson = "\"sender_name\" : " + '"' + sender_name + '"';
            senderNameJson = '{' + senderNameJson + '}';
            return $"[{jsonMessage}, {senderNameJson}]";
        }

        public async Task SendMessage(string jsonMessage, string token) {
            switch (_manager.ValidateAccessToken(token)){
                case TokenStatus.Valid :
                    await PrivateSendMessage(jsonMessage);
                    break; 
                case TokenStatus.Expired :
                    //notify Client
                    Console.WriteLine("Expired");
                    break;
                case TokenStatus.Invalid :
                    Console.WriteLine("Invalid");                    
                    break;
            } 

            
        }

        private async Task PrivateSendMessage(string jsonMessage)
        {

            UserMessage message = new();
            message.FromJson(jsonMessage);
        
            Console.WriteLine($"Chat id: {message.ChatId}");
            ChatList? doesChatExist = null;
            
            if(message.ChatId != null){
                //if the received message contains a chat id means a chat instance has already been created 
                doesChatExist = _context.ChatList.SingleOrDefault(lc => 
                (lc.Id == message.ChatId && message.SenderId == lc.BuyerId) || 
                (lc.Id == message.ChatId && message.SenderId == lc.SellerId));
                
                if(doesChatExist == null){
                    //if the chat still has not been found in the db
                    Console.WriteLine("CHAT DOSE NOT EXIST");
                    return;
                }
            } else if(message.ChatId == null){
                doesChatExist = _context.ChatList.SingleOrDefault(lc => 
                (lc.ProdId == message.ProdId && message.SenderId == lc.BuyerId) || 
                (lc.ProdId == message.ProdId && message.SenderId == lc.SellerId));
                message.ChatId = doesChatExist?.Id;
            }
            
            if(doesChatExist == null){
                //check if the product the message is referring to actually exists
                Product? prod = _context.Product.SingleOrDefault(p => p.Id == message.ProdId);
                
                if(prod != null && message.SenderId != prod.Author){
                    //I'm working on the assumption that if the chat dose not exists yet then the receiver must be the seller

                    //user data 
                    string? receiverId = prod.Author;
                    string? senderId = message.SenderId;

                    //new chat info
                    ChatList chat = new ((Guid) prod.Id!, message.SenderId!, prod.Author!);
                    message.ChatId = chat.Id;
                    //json message that has to be sent to the client
                    string appNotificationData = PrepareMessage(senderId!, message.ToAppMessage(prod.Author!));

                    //check if receiver client is online or offline 
                    string pingCode = await Ping(receiverId!);
                    if(pingCode != ""){   
                        if(await CheckForPingValue(pingCode)){
                            //pinged user offline
                            await SendFirstMessageToOfflineUser(message, receiverId!, prod, chat);
                            return;
                        }else{
                            //pinged user online
                            await SendFirstMessageToOnlineUser(appNotificationData, prod, senderId!, receiverId!, message, chat);
                            return;
                        }
                    } 
                }
            }else{
                var senderId = message.SenderId;
                var receiverId = doesChatExist.BuyerId == senderId ? doesChatExist.SellerId : doesChatExist.BuyerId;

                string pingCode = await Ping(receiverId!);
                if(pingCode != ""){   
                    if(await CheckForPingValue(pingCode)){
                        //pinged user offline
                        await SendMessageToOfflineUser(message);
                    }else{
                        //pinged online
                        UserConnection? receiver = _context.ChatServerConnection.SingleOrDefault(user => user.UserId == receiverId);
                        if(receiver is not null){
                            string appNotificationData = PrepareMessage(senderId!, message.ToAppMessage(receiver.UserId!));
                            Console.WriteLine(appNotificationData);
                            await SendMessageToOnlineUser(appNotificationData, message, receiver!.ConnectionId!);
                            Console.WriteLine("Message mandato");
                        }else{
                            Console.WriteLine("Message non mandato");
                        }  
                    }
                }              
            }
        }
        

        public async Task OnConnect(string token){
            Console.WriteLine("ON CONNECT");
            if(_manager.ValidateAccessToken(token) == TokenStatus.Valid){
                var jwtHandler = new JwtSecurityTokenHandler();
                var jwt = jwtHandler.ReadJwtToken(token);
                JObject payload = JObject.Parse(jwt.Payload.SerializeToJson());
                
                if(payload["sub"] != null){
                    string sub = payload["sub"]!.ToString();
                    UserConnection conn = new(GetConnectionId(), sub);
                    UserConnection? user = _context.ChatServerConnection.SingleOrDefault(cs => cs.UserId == sub);
                    if(user == null){
                        _context.ChatServerConnection.Add(conn);
                        await _context.SaveChangesAsync();
                    }else{
                        _context.ChatServerConnection.Attach(user);
                        user.ConnectionId = GetConnectionId();
                        _context.ChatServerConnection.Update(user);
                        await _context.SaveChangesAsync();
                    } 
                }
            }else{
                Context.Abort();
            }
        }

       public async Task SendImageToClient(string password, string receiver, string jsonData)
        {
            if (BCrypt.Net.BCrypt.Verify(password, _configuration.GetValue<string>("WebApiServerPassword"))){
                List<UserMessage> messages = JsonSerializer.Deserialize<List<UserMessage>>(jsonData)!;
                foreach(UserMessage m in messages){
                    m.Id = Guid.NewGuid();
                    await PrivateSendMessage(m.ObjectToAppMessage(m, receiver));
                }
                Console.WriteLine("messaggi mandati");
            }else{
                Console.WriteLine("password not correct");
            }
            return;
        }
    }
}