using com.ootii.Messages;
using UnityEngine;

public class SendColor : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        // When the sphere collides with something, send a message of
        // type "Color", and include data. The data in this case will
        // be a reference to the material the sphere is currently
        // using (in "Common Assets") The dispatcher will relay the
        // message to listeners of "Color" immediately.
        MessageDispatcher.SendMessage(gameObject, "Color", gameObject.GetComponent<MeshRenderer>().material, 0);
    }
}
