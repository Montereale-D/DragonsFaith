using Unity.Netcode.Components;
using UnityEngine;

namespace Network
{
    public class OwnerNetworkAnimator : NetworkAnimator
    {
        protected override bool OnIsServerAuthoritative()
        {
            return false;
        }
    }
}
