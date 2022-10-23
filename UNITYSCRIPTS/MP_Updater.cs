using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gameing;

public class MP_Updater : MonoBehaviour
{

    public Client client;
    public string nickName = "Patrick der Nullte";
    public string id;
    public int healthPoints;
    public Animator animator;

    void Start(){

        client = GameObject.Find("playerObject").GetComponent<Client>();
    }

    void Update(){
       
        for(int i = 0; i < client.mp_playerList.Count; i++){

            if(client.mp_playerList[i].playerId == id){
                
                transform.position =  Gameing.Vector3.ToUnity(client.mp_playerList[i].mp_Pos);
                transform.localEulerAngles =  Gameing.Vector3.ToUnity(client.mp_playerList[i].mp_Rot);
                healthPoints = client.mp_playerList[i].healthPoints;
                
                if(client.mp_playerList[i].nextAnimation != client.mp_playerList[i].currentAnimation){
                    
                    client.mp_playerList[i].currentAnimation = client.mp_playerList[i].nextAnimation;  
                    animator.SetTrigger(client.mp_playerList[i].currentAnimation);
                    
                    Debug.Log($"[{DateTime.Now.ToString()}]playing animatin: {client.mp_playerList[i].nextAnimation} done");
                }
            }
        }
    }
}
