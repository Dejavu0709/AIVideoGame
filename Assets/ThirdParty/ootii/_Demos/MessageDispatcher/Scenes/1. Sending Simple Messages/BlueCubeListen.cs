using com.ootii.Messages;
using UnityEngine;

public class BlueCubeListen : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // When a message of type "Blue Bounce" is heard, call Jump()
        MessageDispatcher.AddListener("Blue Bounce", Jump);
    }

    private void Jump(IMessage incomingMessage)
    {
        // Pop up a small amount
        gameObject.GetComponent<Rigidbody>().AddForce(0, 100, 0);

        // While not required, this is a good way to be tidy
        // and let others know that the message has been handled
        incomingMessage.IsHandled = true;
    }
}