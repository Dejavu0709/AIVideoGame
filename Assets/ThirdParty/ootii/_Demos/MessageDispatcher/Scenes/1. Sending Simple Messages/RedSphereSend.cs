using com.ootii.Messages;
using UnityEngine;

public class RedSphereSend : MonoBehaviour
{
    void OnCollisionEnter(UnityEngine.Collision collisionInfo)
    {
        // When the sphere collides with something, send a message of
        // type "Red Bounce". The dispatcher will relay the message
        // to listeners of "Red Bounce" immediately.
        MessageDispatcher.SendMessage("Red Bounce");
    }
}
