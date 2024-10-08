using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.Networking;
using System.Threading.Tasks;
using Unity.VisualScripting;

public class DialogueManager2 : MonoBehaviour
{
    private TMP_Text dialogueText;
    private TMP_Text speakerText;
    private gameManagerScript gameManager;
    private int typingDelay;//This is set by the game manager and controls the typing animation delay
    private List<int> nodes;
    private List<string> speakers;
    private List<string> prompts;
    private List<int> nextNodes;
    private int currentNode;
    //The following are all for the typing animation speed
    private bool typingSpeaker; //A boolean that is true when the dialogue is being animated and false otherwise
    private int speakerTextIndex; //The index of which letter the animation is up to in the dialogue
    private int speakerTextLength; //The length of the dialogue, which is used to stop the animation when it is complete
    private string speakerTextTyping; //The string that is being added to for the animation
    private bool typingDialogue;
    private int dialogueTextIndex;
    private int dialogueTextLength;
    private string dialogueTextTyping;
    private bool initialUpdate; //A flag that is true if the initial text update has been started
    private int typingDelayCounter; //Keeps track of how many frames it has been since a typing animation update

    public Animator dialogueAnimator;
#nullable enable
    public Animator? charAnimator;

    private void Start()
    {
        typingSpeaker = false;
        initialUpdate = false;
        typingDelayCounter = 0;
        OpenUI(); // Animates Character and dialogue box on screen
    }

    private void Update()
    {
        //Updates all the text feilds and the manager
        if (!gameManager){
            try{
                gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<gameManagerScript>();
            }
            catch{}
        }
        if (!dialogueText){
            try{
                dialogueText = GameObject.FindGameObjectWithTag("Dialogue").GetComponent<TMP_Text>();
            }
            catch{}
        }
        if (!speakerText){
            try{
                speakerText = GameObject.FindGameObjectWithTag("Speaker").GetComponent<TMP_Text>();
            }
            catch{}
        }
        //Does an initial update when first loaded
        if (currentNode == 1 && prompts != null && !initialUpdate)
        {
            UpdateDialogue();
            initialUpdate = true;
        }

        //Iterate through all the text to animate them on select frames based on the delay
        typingDelayCounter++;
        if (typingSpeaker && typingDelayCounter >= typingDelay && speakers != null)
        {
            speakerTextTyping = speakerTextTyping + speakers[currentNode - 1][speakerTextIndex];
            speakerText.text = speakerTextTyping;
            speakerTextIndex++;
            if (speakerTextIndex == speakerTextLength)
            {
                typingSpeaker = false;
            }
        }
        if (typingDialogue && typingDelayCounter >= typingDelay && prompts != null)
        {
            dialogueTextTyping = dialogueTextTyping + prompts[currentNode - 1][dialogueTextIndex];
            dialogueText.text = dialogueTextTyping;
            dialogueTextIndex++;
            if (dialogueTextIndex == dialogueTextLength)
            {
                typingDialogue = false;
            }
        }
        if (typingDelayCounter >= typingDelay)
        {
            typingDelayCounter = 0;
        }
        
    }


            


    public async void SetUpDialogue(string csvFilePath, int setTypingDelay)
    {
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, csvFilePath);
        UnityWebRequest request = UnityWebRequest.Get(filePath);
        UnityWebRequestAsyncOperation operation = request.SendWebRequest();
        while (!operation.isDone)
        {
            await Task.Yield();
        }
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Cannot load file at " + filePath);
            return;
        }

        //Takes in the data from the csv and puts them in the relevant arrays
        currentNode = 1;
        nodes = new();
        speakers = new();
        prompts = new();
        nextNodes = new();
        typingDelay = setTypingDelay;
        var lines = request.downloadHandler.text.Split('\n');
        for(var nodeNum = 1; nodeNum < lines.Length; nodeNum++){
            var values = lines[nodeNum].Split(",");
            nodes.Add(Convert.ToInt32(values[0]));
            speakers.Add(values[1].Replace('|', ','));
            prompts.Add(values[2].Replace('|', ','));
            nextNodes.Add(Convert.ToInt32(values[3]));
        }
    }

    private void UpdateDialogue()
    {
        //Starts the animation if it has not already started otherwise skips the animation to the end
        if (typingDialogue || typingSpeaker)
        {
            typingDialogue = false;
            typingSpeaker = false;
            speakerText.text = speakers[currentNode - 1];
            dialogueText.text = prompts[currentNode - 1];
        }
        else
        {
            speakerTextIndex = 0;
            speakerTextLength = speakers[currentNode - 1].Length;
            speakerTextTyping = "";
            typingSpeaker = true;
            dialogueTextIndex = 0;
            dialogueTextLength = prompts[currentNode - 1].Length;
            dialogueTextTyping = "";
            typingDialogue = true;
            typingDelayCounter = 10000;
        }
    }

    public void GoToNextDialogue()
    {
        // Updates the current node and tells the game manager to go to the next scene when at the end
        if (!(typingDialogue || typingSpeaker))
        {
            currentNode = nextNodes[currentNode - 1];
        }
        if (currentNode > 0)
        {
            UpdateDialogue();
        }
        else
        {   CloseUI();
            switch (currentNode){
                case 0:
                    gameManager.GoToChoices();
                    break;
                case -1:
                    gameManager.GoToDialogue();
                    break;
                case -2:
                    gameManager.GoToSumary();
                    break;
            }
        }
    }
    public void OpenUI()
    {
        dialogueAnimator.SetBool("isOpen", true);
        if (charAnimator != null)
        {
            charAnimator.SetBool("isOpen", true);
        }
    }

    public void CloseUI()
    {
        dialogueAnimator.SetBool("isOpen", false);
        if (charAnimator != null)
        {
            charAnimator.SetBool("isOpen", false);
        }
    }
}
