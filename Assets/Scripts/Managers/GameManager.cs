using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public CharacterStates playerStates;

    //ʹ�ù۲���ģʽ����ע��
    public void RegisterPlayer(CharacterStates player)
    {
        playerStates = player;
    }
}