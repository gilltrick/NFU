using System;
using System.Text;
using Networking;
using UnityEngine;

namespace Gameing{
    
    public class PlayerAction{

        public string targetId, senderId; //64
        public Gameing.Vector3 mousePosition;//12
        public int ammount, actionId, itemId;//12

        public static byte[] ToBytes(PlayerAction _playerAction){

            byte[] bytes = new byte[88];
            byte[] b_targetId = Encoding.ASCII.GetBytes(_playerAction.targetId);
            byte[] b_senderId = Encoding.ASCII.GetBytes(_playerAction.senderId);
            byte[] b_actionId = BitConverter.GetBytes(_playerAction.actionId);
            byte[] b_ammount = BitConverter.GetBytes(_playerAction.ammount);
            byte[] b_itemId = BitConverter.GetBytes(_playerAction.itemId);
            byte[] b_mousePosition = Gameing.Vector3.ToBytes(_playerAction.mousePosition);

            Buffer.BlockCopy(b_targetId, 0, bytes, 0, 32);
            Buffer.BlockCopy(b_senderId, 0, bytes, 32, 32);
            Buffer.BlockCopy(b_actionId, 0, bytes, 64, 4);
            Buffer.BlockCopy(b_ammount, 0, bytes, 68, 4);
            Buffer.BlockCopy(b_itemId, 0, bytes, 72, 4);
            Buffer.BlockCopy(b_mousePosition, 0, bytes, 76, 12);

            return bytes;
        }

        public static PlayerAction FromBytes(byte[] _bytes){

            PlayerAction playerAction = new PlayerAction();
            playerAction.targetId = Encoding.ASCII.GetString(Utils.BlockCopy(_bytes, 0, 32));
            playerAction.senderId = Encoding.ASCII.GetString(Utils.BlockCopy(_bytes, 32, 32));
            playerAction.actionId = BitConverter.ToInt32(Utils.BlockCopy(_bytes, 64, 4));
            playerAction.ammount = BitConverter.ToInt32(Utils.BlockCopy(_bytes, 68, 4));
            playerAction.itemId = BitConverter.ToInt32(Utils.BlockCopy(_bytes, 72, 4));
            playerAction.mousePosition = Gameing.Vector3.FromBytes(Utils.BlockCopy(_bytes, 76, 12));
            return playerAction;
        }

        public static PlayerAction CustomUpdate(PlayerAction _playerAction, string _targetId, string _senderId, int _actionId, int _ammount, int _itemId, Gameing.Vector3 _mousePosition){

            _playerAction.targetId = _targetId;
            _playerAction.senderId = _senderId;
            _playerAction.actionId = _actionId;
            _playerAction.ammount = _ammount;
            _playerAction.itemId = _itemId;
            _playerAction.mousePosition = _mousePosition;
            return _playerAction;
        }
        /*ANIMATION-CODE for ACTION:200
        0 = isIdle          9 = stopWallRide
        1 = isRunning       
        2 = stopRunning
        3 = startJump
        4 = endJump
        5 = runLeft
        6 = runRight
        7 = leftWallRide
        8 = rightWallRide
        */
    }

    public class PlayerObject{

        public static readonly int PLAYERDATASIZE = 604;
        public string playerId, nickName;
        public Vector3 playerPosition, playerRotation;
        public int healthPoints;
        public int[,] equipment = new int[8,16];

        public PlayerObject(){

            playerId = Utils.CreateRandomId();
            nickName = Utils.CreateRandomId();
            healthPoints = 100;
            playerPosition = new Vector3();
            playerRotation = new Vector3(); 
            equipment = Utils.RandomEquipment();
        }

        public static byte[] ToBytes(PlayerObject _playerObject){

            byte[] b_userId = Encoding.ASCII.GetBytes(_playerObject.playerId);
            byte[] b_nickName = Encoding.ASCII.GetBytes(_playerObject.nickName);
            byte[] b_healthPoints = BitConverter.GetBytes(_playerObject.healthPoints);
            byte[] b_playerPosition = Vector3.ToBytes(_playerObject.playerPosition);
            byte[] b_playerRotation = Vector3.ToBytes(_playerObject.playerRotation);
            
            byte[] b_equipment = new byte[512];
            int equipeIndex = 0;
            for(int i = 0; i < 8; i++){
                for(int j = 0; j < 16; j++){
                    
                    Buffer.BlockCopy(BitConverter.GetBytes(_playerObject.equipment[i,j]), 0, b_equipment, equipeIndex, 4); 
                    equipeIndex+=4;
                }
            }

            byte[] bytes = new byte[PLAYERDATASIZE];
            Buffer.BlockCopy(b_userId,0, bytes, 0, b_userId.Length);
            Buffer.BlockCopy(b_nickName, 0, bytes, 32, b_nickName.Length);
            Buffer.BlockCopy(b_healthPoints, 0, bytes, 64, 4);
            Buffer.BlockCopy(b_playerPosition, 0, bytes, 68, 12);
            Buffer.BlockCopy(b_playerRotation, 0, bytes, 80, 12);
            Buffer.BlockCopy(b_equipment, 0, bytes, 92, 512);
            return bytes;
        }

        public static PlayerObject FromBytes(byte[] _bytes){

            PlayerObject playerObject = new PlayerObject();

            byte[] b_userId = new byte[32];
            byte[] b_nickName = new byte[32];
            byte[] b_healthPoints = new byte[4];
            byte[] b_playerPosition = new byte[12];
            byte[] b_playerRotation = new byte[12];
            byte[] b_equipment = new byte[512];

            Buffer.BlockCopy(_bytes, 0, b_userId, 0, b_userId.Length);
            Buffer.BlockCopy(_bytes, 32, b_nickName, 0, b_nickName.Length);
            Buffer.BlockCopy(_bytes, 64, b_healthPoints, 0, 4);
            Buffer.BlockCopy(_bytes, 68, b_playerPosition, 0, 12);
            Buffer.BlockCopy(_bytes, 80, b_playerRotation, 0, 12);
            Buffer.BlockCopy(_bytes, 92, b_equipment, 0, 512);

            playerObject.playerId = Encoding.ASCII.GetString(b_userId);
            playerObject.nickName = Encoding.ASCII.GetString(b_nickName);
            playerObject.healthPoints = BitConverter.ToInt32(b_healthPoints);
            playerObject.playerPosition = Vector3.FromBytes(b_playerPosition);
            playerObject.playerRotation = Vector3.FromBytes(b_playerRotation);

            int equipeIndex = 0;
            for(int i = 0; i < 8; i++){
                for(int j = 0; j < 16; j++){

                    playerObject.equipment[i,j] = BitConverter.ToInt32(new byte[]{b_equipment[equipeIndex],b_equipment[equipeIndex+1],b_equipment[equipeIndex+2],b_equipment[equipeIndex+3]});
                    equipeIndex+=4;
                }
            }

            return playerObject;
        }
    }

    public class Vector3{

        public float x, y, z;

        public Vector3(float _x, float _y, float _z){

            x = _x;
            y = _y;
            z = _z;
        }

        public Vector3(){

            x = 0f;
            y = 0f;
            z = 0f;
        }

        public static byte[] ToBytes(Vector3 _vector3){

            byte[] bytes = new byte[12];
            Buffer.BlockCopy(BitConverter.GetBytes(_vector3.x), 0, bytes, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(_vector3.y), 0, bytes, 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(_vector3.z), 0, bytes, 8, 4);
            return bytes;
        }

        public static Vector3 FromBytes(byte[] _bytes){

            Vector3 vector3 = new Vector3();
            vector3.x = BitConverter.ToSingle(new byte[]{_bytes[0],_bytes[1],_bytes[2],_bytes[3]});
            vector3.y = BitConverter.ToSingle(new byte[]{_bytes[4],_bytes[5],_bytes[6],_bytes[7]});
            vector3.z = BitConverter.ToSingle(new byte[]{_bytes[8],_bytes[9],_bytes[10],_bytes[11]});
            return vector3;
        }

        public static UnityEngine.Vector3 ToUnity(Gameing.Vector3 _vector3){

            //UnityEngine.Vector3 v3 = new UnityEngine.Vector3(_vector3.x, _vector3.z, _vector3.z);//ich checks grad nicht...
            UnityEngine.Vector3 v3 = new UnityEngine.Vector3();
            v3.x = _vector3.x;
            v3.y = _vector3.y;
            v3.z = _vector3.z;
            return v3;
        }

        public static Gameing.Vector3 ToGaming(UnityEngine.Vector3 _vector3){

            Gameing.Vector3 v3 = new Gameing.Vector3(_vector3.x, _vector3.y, _vector3.z);
            return v3;
        }
    }
}