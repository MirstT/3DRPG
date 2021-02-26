using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewData", menuName = "Character States/Data")]
public class CharacterData_SO : ScriptableObject
{
    [Header("States Info")]
    public int maxHealth;

    public int currentHealth;
    public int baseDefence;
    public int currentDefence;
}