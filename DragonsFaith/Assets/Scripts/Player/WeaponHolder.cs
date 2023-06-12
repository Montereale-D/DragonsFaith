using System.Collections;
using System.Collections.Generic;
using Inventory;
using Inventory.Items;
using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    private GameObject _cloneWeapon;
    public Weapon defaultWeapon;
    
    public void SetUpWeapon()
    {
        var weapon = InventoryManager.Instance.GetWeapon();
        if (!weapon) weapon = defaultWeapon;
        _cloneWeapon = Instantiate(weapon.weaponObject, transform);
        _cloneWeapon.transform.position = transform.position;
    }

    public void DestroyWeapon()
    {
        Destroy(_cloneWeapon);
    }
}
