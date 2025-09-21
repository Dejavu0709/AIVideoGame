using com.ootii.Messages;
using UnityEngine;

public class BlueSphereSend : MonoBehaviour
{
    void OnCollisionEnter(UnityEngine.Collision collisionInfo)
    {
        // When the sphere collides with something, send a message of
        // type "Blue Bounce". The dispatcher will relay the message
        // to listeners of "Blue Bounce" after exactly half a second.
        MessageDispatcher.SendMessage("Blue Bounce", 0.5f);
    }
}
