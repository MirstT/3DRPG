using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public CharacterStates playerStates;

    //使用观察者模式反向注册
    public void RegisterPlayer(CharacterStates player)
    {
        playerStates = player;
    }
}