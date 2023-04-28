using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttributeType
{
    Strength = 0,
    Dexterity = 1,
    Stamina = 2,
    Charisma = 3,
    Manipulation = 4,
    Composure = 5,
    Intelligence = 6,
    Wits = 7,
    Resolve = 8
}

public enum AttributeScore
{
    Poor = 1,
    Average = 2,
    Good = 3,
    Excellent = 4,
    Superhuman = 5
}

[Serializable]
public struct Attribute
{
    public AttributeType attribute;
    public AttributeScore score;

    public Attribute(AttributeType attribute, AttributeScore score)
    {
        this.attribute = attribute;
        this.score = score;
    }
}

[CreateAssetMenu(fileName = "NewCharacter", menuName = "ScriptableObjects/Character")]
public class CharacterSO : ScriptableObject
{
    public string characterName;
    public RuntimeAnimatorController animatorController;
    public List<Attribute> attributes;

    // Called to reset default values
    private void Reset()
    {
        characterName = "New Character";

        attributes.Clear();
        for (int i = 0; i < 9; i++)
        {
            Attribute attribute = new Attribute((AttributeType)i, AttributeScore.Average);
            attributes.Add(attribute);
        }
    }
}