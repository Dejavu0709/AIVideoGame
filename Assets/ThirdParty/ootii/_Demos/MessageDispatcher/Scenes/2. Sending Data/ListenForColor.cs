using com.ootii.Messages;
using UnityEngine;

public class ListenForColor : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // Tell the dispatcher that this object wants to recieve messages of type "Color".
        MessageDispatcher.AddListener("Color", ColorHandler);
    }

    private void ColorHandler(IMessage incomingMessage)
    {
        // When a message of type "Color" is recieved, the message's data is expected to be a reference to a material.
        // Set this objects material to the material from incomingMessage.Data
        gameObject.GetComponent<MeshRenderer>().material = (Material)incomingMessage.Data;
        Debug.Log("Changed to " + (Material)incomingMessage.Data);

        // While not required, this is a good way to be tidy
        // and let others know that the message has been handled
        incomingMessage.IsHandled = true;
    }
}
