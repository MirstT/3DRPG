using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public CharacterStates playerStates;

    //使用观察者模式反向注册
    public void RegisterPlayer(CharacterStates player)
    {
        playerStates = player;
    }
}