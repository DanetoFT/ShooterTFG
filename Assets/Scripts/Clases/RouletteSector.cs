using UnityEngine;

[System.Serializable]
public class RouletteSector
{
    [Header("Weapon")]
    public string weaponName;
    public GameObject weaponObject;

    [Header("Roulette")]
    [Min(0.01f)]
    public float weight = 1;

    public Color color = Color.white;

    [Header("Ammo")]
    [Min(1)]
    public int maxAmmo = 30;

    [HideInInspector]
    public int currentAmmo;

    [HideInInspector]
    public float angle;

    [HideInInspector]
    public float startAngle;

    [HideInInspector]
    public float endAngle;

    [HideInInspector]
    public float middleAngle;

    [HideInInspector]
    public float originalAngle;
}