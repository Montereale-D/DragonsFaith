using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Audio/SoundBank")]
public class SoundBank : ScriptableObject
{
    [Header ("Player")]
    public AudioClip PlayerWalk;
    public AudioClip PlayerDash;
    public AudioClip PlayerHurt;
    public AudioClip PlayerDie;
    public AudioClip PlayerReward;

    [Header("Knife")] 
    public AudioClip knifeAttack;
    
    [Header("Pistol")]
    public AudioClip pistolAttack;
    public AudioClip pistolReload;

    [Header("AssaultRifle")]
    public AudioClip assaultRifleAttack;
    public AudioClip assaultRifleReload;

    [Header("Shotgun")]
    public AudioClip shotgunAttack;
    public AudioClip shotgunReload;
    
    [Header("SniperRifle")]
    public AudioClip sniperRifleAttack;
    public AudioClip sniperRifleReload;

    [Header("UI")]
    public AudioClip ClickUIButton;
    public AudioClip OverUIButton;

    [Header("Music")]
    public AudioClip SoundTrackMenu;
    public AudioClip SoundTrackExplore;
    public AudioClip SoundTrackCombat;
    
    [Header("Dragon")]
    public AudioClip fireBreath;

    [Header("Other")]
    public AudioClip obstacleDestruction;
    public AudioClip barrelExplosion;
    public AudioClip openingChest;
    public AudioClip openingGate;
    public AudioClip openingHiddenArea;
    public AudioClip usingItem;
    public AudioClip spotted;

}