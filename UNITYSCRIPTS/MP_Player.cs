using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gameing;
public class MP_Player : MonoBehaviour
{
    public string playerId, nickName;
    public int healthPoints;
    public Gameing.Vector3 mp_Pos, mp_Rot;
    public static List<string> animationNameList = new List<string>{"isIdle", "isRunning", "stopRunning", "startJump", "endJump", "runLeft", "runRight", "leftWallRide", "rightWallRide", "stopWallRide", "runBackward"};
    public string nextAnimation = "isIdle",
                  currentAnimation;
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
