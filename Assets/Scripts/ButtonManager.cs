using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonManager : MonoBehaviour
{

    public GameObject nextButton;

    public void DialogueOver(){
        nextButton.SetActive(true);
    }
}
