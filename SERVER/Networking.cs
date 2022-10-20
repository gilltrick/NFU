using System;
using System.Text;
using System.Security.Cryptography;
using Gameing;
using System.Net;
using System.Net.Sockets;


namespace Networking{

    public class Server{

        public static List<Socket> clientSocketList = new List<Socket>();
        public static List<ClientObject> clientObjectList = new List<ClientObject>();
        static List<PlayerObject> playerObjectList = new List<PlayerObject>();
        public static string lobbyId;
        static Socket socket;
        
        static int port = 8112;//4321;
        public Server(){Server server = new Server();}
        

        public static void Start(){
            
            lobbyId = Utils.CreateRandomId();
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Any, port));
            socket.Listen(4);
            Task.Run(()=>{AcceptLoop();});
            Task.Run(()=>{BroadcastLoop();});
            Console.WriteLine("[S] Press any key to exit");
            Console.ReadKey();
        }

        static Socket GetSocketByUserId(string _id){

            for(int i = 0; i < clientObjectList.Count; i++)
                if(clientObjectList[i].id == _id)
                    return clientObjectList[i].socket;
            return null;
        }

        public static void BroadcastLoop(){

            while(true){
                if(clientObjectList.Count >0){
                    for(int i = 0; i < clientObjectList.Count;i++){
                        for(int j = 0; j < clientObjectList.Count;j++){
                            
                            if(clientObjectList[j].id != clientObjectList[i].id){
                                
                                Socket socket = GetSocketByUserId(clientObjectList[j].id);
                                Packet packet = new Packet();
                                packet.data = PlayerObject.ToBytes(clientObjectList[i].playerObject);
                                Header header = new Header(packet.data.Length+Header.HEADERSIZE,15,lobbyId,0,0);
                                packet.header = header;
                                byte[] data =  Packet.PacketToBytes(packet);
                                socket.Send(data,data.Length,0);
                                //Console.WriteLine($" data [pos >> x: {clientObjectList[i].playerObject.playerPosition.x}y: {clientObjectList[i].playerObject.playerPosition.y} z:{clientObjectList[i].playerObject.playerPosition.z}] for clientObjectList[{i}].playerObject.nickName: {clientObjectList[i].playerObject.nickName} send to {clientObjectList[j].playerObject.nickName}");
                            }
                        }
                    }                    
                }
                //Console.WriteLine("Waiting for nex broadcast cycle");
                Thread.Sleep(30);
            }
        }

        public static void AcceptLoop(){
            
            Console.WriteLine("[S] Socket count: " + clientSocketList.Count.ToString());
            while(true){

                Socket clientSocket = socket.Accept();
                ClientObject clientObject = new ClientObject();
                clientObject.socket = clientSocket;
                clientObject.id = Utils.CreateRandomId();
                clientObject.playerObject.playerId = clientObject.id; 
                clientSocketList.Add(clientSocket);
                Console.WriteLine("[S] Socket count: " + clientSocketList.Count.ToString());
                SendClientId(clientSocket, clientObject.id);
                Task.Run(()=>{ReceiveConfirmation(clientObject);});
            }
        }

        public static void SendClientId(Socket _socket, string _clientId){


            Packet packet = new Packet();
            packet.data = Encoding.ASCII.GetBytes(_clientId);
            packet.header = new Header(packet.data.Length+48,2,"server",0,0);
            byte[] data = Packet.PacketToBytes(packet);
            _socket.Send(data, data.Length, 0);
        }

        public static void ReceiveConfirmation(ClientObject _clientObject){
            
            bool unconfirmed = true;
            byte[] dataReceived = new byte[Packet.PACKETSIZE];
            string clientId = string.Empty;
            while(unconfirmed){
                try{
                    int rec = _clientObject.socket.Receive(dataReceived, 0, dataReceived.Length, SocketFlags.None);
                    byte[] tempBytes = new byte[rec];
                    Array.Copy(dataReceived, tempBytes, rec);
                    Packet packet = Packet.BytesToPacket(tempBytes);
                    PlayerObject playerObject = PlayerObject.FromBytes(packet.data);
                    try{
                        //eigentlich will ich die id in packet.data schicken aber auch wenn der string und die datei gleich sind frag nicht aber es geht nicht. so dumm hab viel zeit verschwendet. fürs erste. will aber den fehler verstehen.
                        clientId = Encoding.ASCII.GetString(packet.header.senderId);
                        if(clientId == _clientObject.id){
                            
                            _clientObject.playerObject = playerObject;
                            clientObjectList.Add(_clientObject);
                            playerObjectList.Add(playerObject);
                            Console.WriteLine($"clientObjectList.Count: {clientObjectList.Count}");
                            unconfirmed = false;
                            Console.WriteLine($"[S] Client wit id: {clientId} confirmed");
                            Task.Run(()=>{ReceiveClientData(_clientObject);});
                        }
                        //else check database if user with the id is known. if yes, log him in
                    }   
                    catch (Exception ex){
                        Console.WriteLine($"[S] Cant read client id data >> {ex}");
                        Thread.Sleep(100);
                        return;
                    }
                }
                catch(Exception ex){
                    Console.WriteLine($"[S] Cant receive data >> {ex}");
                    Thread.Sleep(100);
                    return;
                }
                return;
            }
        }

        public static void ReceiveClientData(ClientObject _clientObject){

            byte[] dataReceived = new byte[Packet.PACKETSIZE];
            while(true){
                try{
                    int rec = _clientObject.socket.Receive(dataReceived, 0, dataReceived.Length, SocketFlags.None);
                    byte[] tempBytes = new byte[rec];
                    Array.Copy(dataReceived, tempBytes, rec);
                    Packet packet = new Packet();
                    packet = Packet.BytesToPacket(dataReceived);
                    int command = BitConverter.ToInt32(packet.header.command);
                    if(command == 0){
                        
                        //Console.WriteLine($"[S] Received Message :{Encoding.ASCII.GetString(packet.data)}");
                    }

                    if(command == 15){
                        
                        _clientObject.playerObject = PlayerObject.FromBytes(packet.data);
                            
                    }
                    //get playerAction
                    if(command == 16){

                        PlayerAction playerAction = PlayerAction.FromBytes(packet.data);
                        Console.WriteLine($"playerAction >> targetId: {playerAction.targetId} || ammount {playerAction.ammount} || actionId {playerAction.actionId} || itemId {playerAction.itemId} || mousePos.x {playerAction.mousePosition.x}");

                        for(int i = 0; i < clientObjectList.Count; i++){

                            if(clientObjectList[i].id == playerAction.targetId){

                                
                                clientObjectList[i].socket.Send(dataReceived, rec, 0);
                            }
                        }
                    }

                    if(command == 99){

                        Disconnect(Encoding.ASCII.GetString(packet.data));
                    }
                }
                catch(Exception ex){
                    Console.WriteLine($"[S] Cant receive data >> {ex}");
                }
            }
            Console.WriteLine("Why did it stop");
        }

        public static void SendMessage(ClientObject _clientObject, int _command, string _message){

            if(_clientObject.socket.Connected){

                Packet packet = new Packet();
                packet.data = Encoding.ASCII.GetBytes(_message);
                packet.header = new Header(packet.data.Length+48,_command,"SERVER_COMMAND_ID_XXXXX_STAMP_XX",0,1);
                byte[] data = Packet.PacketToBytes(packet);
                _clientObject.socket.Send(data, data.Length, 0);
                Console.WriteLine($"[S] Sending message[{_command}]: {_message} to user: {_clientObject.id}");
            }
        }

        static void Disconnect(string _clientId){

            for(int i = 0; i < clientObjectList.Count; i++){

                if(clientObjectList[i].id == _clientId){

                    clientObjectList[i].socket.Disconnect(false);
                    clientObjectList.Remove(clientObjectList[i]);
                    Console.WriteLine($"[S] Client with id {_clientId} disconnected");
                    return;
                }
            }
        }
    }


    public class ClientObject{

        public Socket socket;
        public string id, lobbyId;
        public PlayerObject playerObject;

        public ClientObject(){

            playerObject = new PlayerObject();
        }
    }


    public class Packet{

        public static readonly int PACKETSIZE = 4098;
        public Header header;
        public byte[] data;

        public Packet(){

            header = new Header();
            data = new byte[PACKETSIZE];
        }

        public static byte[] PacketToBytes(Packet _packet){

            byte[] bytes = new byte[_packet.data.Length+Header.HEADERSIZE];
            Buffer.BlockCopy(Header.HeaderToBytes(_packet.header), 0, bytes, 0, Header.HEADERSIZE);
            Buffer.BlockCopy(_packet.data, 0, bytes, Header.HEADERSIZE, _packet.data.Length);
            return bytes;
        }

        public static Packet BytesToPacket(byte[] _bytes){

            byte[] len = new byte[] {_bytes[0],_bytes[1],_bytes[2],_bytes[3]};
            byte[] com = new byte[] {_bytes[4],_bytes[5],_bytes[6],_bytes[7]};
            byte[] uId = new byte[] {_bytes[8],_bytes[9],_bytes[10],_bytes[11],_bytes[12],_bytes[13],_bytes[14],_bytes[15],_bytes[16],_bytes[17],_bytes[18],_bytes[19],_bytes[20],_bytes[21],_bytes[22],_bytes[23],
                                     _bytes[24],_bytes[25],_bytes[26],_bytes[27],_bytes[28],_bytes[29],_bytes[30],_bytes[31],_bytes[32],_bytes[33],_bytes[34],_bytes[35],_bytes[36],_bytes[37],_bytes[38],_bytes[39]};
            byte[] ind = new byte[] {_bytes[40],_bytes[41],_bytes[42],_bytes[43]};
            byte[] exp = new byte[] {_bytes[44],_bytes[45],_bytes[46],_bytes[47]};
            
            Packet packet = new Packet();
            packet.header = new Header(BitConverter.ToInt32(len)+Header.HEADERSIZE,BitConverter.ToInt32(com),Encoding.ASCII.GetString(uId),BitConverter.ToInt32(ind),BitConverter.ToInt32(exp));
            Buffer.BlockCopy(_bytes, Header.HEADERSIZE, packet.data, 0,_bytes.Length-Header.HEADERSIZE);
            return packet;
        }

        public static string GetString(Packet _packet){

            int datalen = BitConverter.ToInt32(_packet.header.fileSize)-Header.HEADERSIZE;
            Console.WriteLine(datalen);
            byte[] data = new byte[datalen];
            Buffer.BlockCopy(_packet.data, Header.HEADERSIZE, data, 0, datalen);
            string str = Encoding.ASCII.GetString(data);
            return str;
        }
    }


    public class Header{

        public static readonly int HEADERSIZE = 48;
        public byte[] fileSize;
        public byte[] command;
        public byte[] senderId;
        public byte[] index;
        public byte[] indexCount;

        public Header(){

            fileSize = new byte[4];
            command = new byte[4];
            senderId = new byte[32];
            index = new byte[4];
            indexCount = new byte[4];
        }

        public Header(int _fileSize, int _command, string _senderId, int _index, int _indexCount){

            if(_senderId.Length < 32){
                int len = _senderId.Length;
                for(int i = 0; i < 32 - len; i++){

                    _senderId+="*"; 
                }
            }

            fileSize = BitConverter.GetBytes(_fileSize);
            command = BitConverter.GetBytes(_command);
            senderId = Encoding.ASCII.GetBytes(_senderId);
            index = BitConverter.GetBytes(_index);
            indexCount = BitConverter.GetBytes(_indexCount);
        }

        public static byte[] HeaderToBytes(Header _header){

            byte[] headerBytes = new byte[HEADERSIZE];
            Buffer.BlockCopy(_header.fileSize, 0, headerBytes, 0, 4);
            Buffer.BlockCopy(_header.command, 0, headerBytes, 4, 4);
            Buffer.BlockCopy(_header.senderId, 0, headerBytes, 8, 32);
            Buffer.BlockCopy(_header.index, 0, headerBytes, 40, 4);
            Buffer.BlockCopy(_header.indexCount, 0, headerBytes, 44, 4);
            return headerBytes;
        }
    }


    public class Utils{

        public static Random random = new Random();

        public static string CreateMD5Hash(string input){

            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < hashBytes.Length; i++){

                sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString();
        }

        public static string CreateRandomId(){

            MD5 md5 = MD5.Create();
            string id = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            byte[] bytes = Encoding.ASCII.GetBytes(id);
            byte[] hashedBytes = md5.ComputeHash(bytes);
            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < hashedBytes.Length; i++){

                sb.Append(hashedBytes[i].ToString("X2"));
            }
            return sb.ToString();
        }

        public static string EquipmentToString(int[,] _quipmentSlots){
            string str = "";
            for(int i = 0; i < 8; i++){

                for(int j = 0; j < 16; j++){

                    str +=$"[{_quipmentSlots[i,j]}]";
                }
                str +="\n";
            }
            return str;
        }

        public static int[,] RandomEquipment(){
            
            int[,] equipmentSlots = new int[8,16];
            for(int i = 0; i < 8; i++){

                for(int j = 0; j < 16; j++){

                    equipmentSlots[i,j] = random.Next(0,1024);
                }
            }
            return equipmentSlots;
        }

        public static byte[] BlockCopy(byte[] _bytes, int _offSet, int _count){

            byte[] bytes = new byte[_count];
            Buffer.BlockCopy(_bytes, _offSet, bytes, 0, _count);
            return bytes;
        }   
    }
}