using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Networking;
using Gameing;
using UnityEngine;

public class Client : MonoBehaviour{

        Socket socket;
        string address = "62.158.198.198";//"127.0.0.1";
        int port = 8112;//4321;
        public Packet packet;
        public string nickName = "Patrick der Erste";
        public string clientId = string.Empty;
        public string lobbyId, tempLobbyId;
        public PlayerObject playerObject, mp_playerObject;
        public List<MP_Player> mp_playerList = new List<MP_Player>();
        public Manager manager;

        public void Start(){

            lobbyId = "lobbyId";
            tempLobbyId = lobbyId;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            mp_playerObject = new PlayerObject();
            playerObject = new PlayerObject();
            playerObject.playerId = "invalid";//hier lade ich die id vom speicher 
            playerObject.nickName = nickName;
            playerObject.equipment = Utils.RandomEquipment();

            ConnectToServer();
            ReceiveClientId();
            SendConfirmation();
            Task.Run(()=>{ReceiveServertData();});
            Task.Run(()=>{SendUserData();});
        }

        public void ConnectToServer(){

            Debug.Log($"[C] Try to connect to {address}:{port}");
            int attempts = 0;
            while(!socket.Connected){

                try{
                    socket.Connect(address,port);
                    Debug.Log($"[C] Connected to {address}:{port}");
                }
                catch{
                    Debug.Log($"[C] Can't connect to {address}:{port}. Trying again...");
                    attempts++;
                    if(attempts > 10){
                        Debug.Log($"[C] Can't connect to {address}:{port}.");
                        return;
                    }
                    Thread.Sleep(1000);
                }
            }
        }

        public void ReceiveClientId(){

            byte[] dataReceived = new byte[Packet.PACKETSIZE];
            while(clientId == "" && socket.Connected){

                try{
                    int rec = socket.Receive(dataReceived, 0, dataReceived.Length, SocketFlags.None);
                    byte[] tempBytes = new byte[rec];
                    Array.Copy(dataReceived, tempBytes, rec);
                    Packet packet = Packet.BytesToPacket(tempBytes);
                    clientId = Packet.GetString(packet);
                    if(playerObject.playerId == "invalid"){
                        Debug.Log("Setting received id to client id");
                        playerObject.playerId = clientId;
                    }
                    Debug.Log($"[C] Received client id: {clientId}");
                    return;
                }
                catch{
                    Debug.Log("[C] Waiting for client id");
                }
                Thread.Sleep(1000);
            }
        }
        public void SendConfirmation(){

            Packet packet = new Packet();
            packet.data = PlayerObject.ToBytes(playerObject);
            packet.header = new Header(packet.data.Length+Header.HEADERSIZE,15,clientId,0,1);
            byte[] data = Packet.PacketToBytes(packet);
            socket.Send(data, data.Length, 0);
            Debug.Log($"[C] Confirmation for id: {clientId} send to server.");
        }

        public void SendUserData(){
            
            while(socket.Connected){
                
                Packet packet = new Packet();
                packet.data = PlayerObject.ToBytes(playerObject);
                packet.header = new Header(packet.data.Length+Header.HEADERSIZE,15,clientId,0,1);
                byte[] data = Packet.PacketToBytes(packet);
                socket.Send(data, data.Length, 0);
                Thread.Sleep(30);
            }
            Debug.Log($"[C] Stoped Sending userdata: {clientId}");
        }

        public void SendPlayerAction(PlayerAction _playerAction){
Debug.Log($"PlayerAction Sendet >> targetId: {_playerAction.targetId} _playerAction.ammount: {_playerAction.ammount}");
            Packet packet = new Packet();
            packet.data = PlayerAction.ToBytes(_playerAction);
            packet.header = new Header(packet.data.Length+Header.HEADERSIZE,16,clientId,0,1);
            byte[] data = Packet.PacketToBytes(packet);
            socket.Send(data, data.Length, 0);
Debug.Log($"PlayerAction Sendet sucessfully >> targetId: {_playerAction.targetId} _playerAction.ammount: {_playerAction.ammount}");
        }

        public void ReceiveServertData(){

            byte[] dataReceived = new byte[Packet.PACKETSIZE];
            while(socket.Connected){
                
                try{
                    int rec = socket.Receive(dataReceived, 0, dataReceived.Length, SocketFlags.None);
                    byte[] tempBytes = new byte[rec];
                    Array.Copy(dataReceived, tempBytes, rec);
                    Packet packet = new Packet();
                    packet = Packet.BytesToPacket(dataReceived);
                    int command = BitConverter.ToInt32(packet.header.command);
                    
                    if(command == 0){

                        //Debug.Log($"[S] Received Message :{Encoding.ASCII.GetString(packet.data)}");
                    }
                    //receive lobby id
                    if(command == 12){
                        
                        lobbyId = Packet.GetString(packet);
                    }
                    //get lobby data update
                    if(command == 15){

                        mp_playerObject = PlayerObject.FromBytes(packet.data);

                        if(mp_playerObject.playerId != clientId){
                            
                            if(mp_playerList.Count <= 0){

                                MP_Player mp_player = new MP_Player();
                                mp_player.playerId = mp_playerObject.playerId;
                                mp_player.nickName = mp_playerObject.nickName;
                                mp_playerList.Add(mp_player);
                            }
                            for(int i = 0; i < mp_playerList.Count; i++){

                                if(mp_playerList[i].playerId == mp_playerObject.playerId){

                                    mp_playerList[i].mp_Pos = mp_playerObject.playerPosition;
                                    mp_playerList[i].mp_Rot = mp_playerObject.playerRotation;
                                    mp_playerList[i].healthPoints = mp_playerObject.healthPoints;
                                }
                            }
                        }
                    }

                    if(command == 16){

                        PlayerAction playerAction = new PlayerAction();
                        playerAction = PlayerAction.FromBytes(packet.data);
                        Debug.Log($"playerAction >> targetId {playerAction.targetId} actionId >> {playerAction.actionId}");
                        if(playerAction.actionId == 100){

                            manager.playerController.healthPoints -= playerAction.ammount;
                        }
                        if(playerAction.actionId == 200){
Debug.Log("200");
                            for(int i = 0; i < mp_playerList.Count; i++){
                                Debug.Log("suche...");
                                if(mp_playerList[i].playerId == playerAction.senderId){
                                    Debug.Log("im client siehts gut aus");
                                    mp_playerList[i].nextAnimation = MP_Player.animationNameList[playerAction.itemId];
                                }
                            }
                        }
                    }

                    if(command == 99){

                        socket.Disconnect(false);
                    }
                }

                catch(Exception ex){
                    Debug.Log($"[S] Cant receive data >> {ex}");
                }
            }
        }
    }