using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    private void Update()
    {
        if (!IsOwner) return;
        
        var moveDir = new Vector3(0, 0, 0);
        var speed = 3f;

        if (Input.GetKey(KeyCode.W)) moveDir.y = +1f;
        if (Input.GetKey(KeyCode.S)) moveDir.y = -1f;
        if (Input.GetKey(KeyCode.A)) moveDir.x = -1f;
        if (Input.GetKey(KeyCode.D)) moveDir.x = +1f;

        transform.position += moveDir * (speed * Time.deltaTime);
    }
}
