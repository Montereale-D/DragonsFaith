using System;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public enum AttributeType
    {
        Strength = 1,
        Dexterity = 2,
        Constitution = 3,
        Intelligence = 4,
        Agility = 5
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
        public void Reset()
        {
            characterName = "New Character";

            attributes.Clear();
            for (int i = 0; i < 9; i++)
            {
                Attribute attribute = new Attribute((AttributeType)i, AttributeScore.Average);
                attributes.Add(attribute);
            }
        }

        public AttributeScore GetAttributeScore(AttributeType attributeType)
        {
            foreach (var attribute in attributes)
            {
                if (attribute.attribute == attributeType)
                    return attribute.score;
            }

            throw new Exception("Attribute not found");
        }
    }
}