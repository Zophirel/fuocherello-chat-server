# Fuocherello Chat Server

This is a SignalR chat server that allows real-time communication between Fuocherello Flutter App (clients)

## Features

- **Real-time messaging:** Clients can send and receive messages in real-time.
- **Authentication:** Token-based authentication using JWT tokens.
- **Offline messaging:** Support for sending messages to users who are currently offline.
- **Image messaging:** Ability to send images as messages.

## How To Build
    ├── /data
    ├── /Hubs
    ├── /Models
    ├── /Properties
    ├── /Singleton
    ├── /wwwroot 
    ├── appsetting.json                 (𝗣𝗿𝗲𝘀𝗲𝗻𝘁 𝗯𝘂𝘁 𝗶𝗻𝗰𝗼𝗺𝗽𝗹𝗲𝘁𝗲)
    ├── appsetting.Development.json     (𝗣𝗿𝗲𝘀𝗲𝗻𝘁 𝗯𝘂𝘁 𝗶𝗻𝗰𝗼𝗺𝗽𝗹𝗲𝘁𝗲)
    ├── key                             (𝗺𝗶𝘀𝘀𝗶𝗻𝗴)
    └── SslCertificate.pfx              (𝗺𝗶𝘀𝘀𝗶𝗻𝗴)
 
  ### 1. <a href="https://github.com/Zophirel/fuocherello-chat-server/blob/main/appsettings.json"> appsetting.json </a> / <a href="https://github.com/Zophirel/fuocherello-chat-server/blob/main/appsettings.Development.json"> appsettings.Development.json </a>
    {
      "Kestrel": {
      (SSL Cerificate) 
      //add an SSL certificate or remove this key
        "Certificates": {
          "Default": {
            "Path": "",
            "Password": ""
          }
        }
      },
      
      "Logging": {
        "LogLevel": {
          "Default": "Information",
          "Microsoft.AspNetCore": "Warning"
        }
      },
      
      //BCrypt hashed version of the password expected 
      //to recieve when rest api make requests to the SignalR server  
      "WebApiServerPassword" : "",
      
      "AllowedHosts": "*",
      //db connection
      
      "ConnectionStrings": {
        "DbContext": "Username=user; Password=pass; Database=dbname; Host=dbip; Port=5432;"
      }
    }

  ### 2. Key
  SignalR server and REST API server share <a href="https://github.com/Zophirel/fuocherello-back-end?tab=readme-ov-file#2-key-missing-file"> the same RSA key </a>, but the SignalR use it only to validate token sent by the clients
  
  ### 3. SSL Certificate
  It is possible to make an SSL Certificate <a href="https://github.com/Zophirel/fuocherello-back-end?tab=readme-ov-file#3-sslcertificatepfx-missing-file"> following these steps </a>
  
  ### 4. Database
  The Database is shared with the REST API server, the dump and a dbml rapresentation of the db can be found <a href="https://github.com/Zophirel/fuocherello-back-end?tab=readme-ov-file#3-sslcertificatepfx-missing-file"> Here </a> 

  ## Contributing
  Contributions are welcome! Please feel free to open issues or submit pull requests to improve this project

  ## License
  This project is licensed under the [MIT License](LICENSE)
