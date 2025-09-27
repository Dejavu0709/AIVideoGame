using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableFromAnimation : MonoBehaviour
{
    public void Deactivate() {
        gameObject.SetActive(false);
    }
}
