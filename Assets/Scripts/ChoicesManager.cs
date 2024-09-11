using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Transactions;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ChoiceManager : MonoBehaviour
{
    private List<string> unseenCharacters = new() {"Jess", "Frank", "Wilson", "Evan", "Olivia"};
    private TMP_Text choice1TextObject;
    private TMP_Text choice2TextObject;
    private TMP_Text choice3TextObject;
    private TMP_Text promptTextObject;
    private gameManagerScript gameManager;
    private int typingDelay; //This is set by the game manager and controls the typing animation delay
    private int currentNode;
    private List<int> nodes = new();
    private List<string> prompts = new();
    private List<string> choice1Text = new();
    private List<string> choice2Text = new();
    private List<string> choice3Text = new();
    private List<int> choice1NextNode = new();
    private List<int> choice2NextNode = new();
    private List<int> choice3NextNode = new();
    private List<int> FinancialChanges = new();
    private List<int> TeamMoralChanges = new();
    private List<int> MoralityScore1Changes = new();
    private List<int> MoralityScore2Changes = new();
    private List<int> MoralityScore3Changes = new();
    private List<int> MoralityScore4Changes = new();
    private List<int> MoralityScore5Changes = new();
    private List<int> MoralityScore6Changes = new();
    private List<string> presentCharacters = new();
    private int resultsStart; //Marks where the choices end and the results nodes start
    //The following are all for the typing animation speed
    private bool typingPrompt; //A boolean that is true when the prompt is being animated and false otherwise
    private int promptTextIndex; //The index of which letter the animation is up to in the prompt
    private int promptTextLength; //The length of the prompt, which is used to stop the animation when it is complete
    private string promptTextTyping; //The string that is being added to for the animation
    private bool typingChoice1;
    private int choice1TextIndex;
    private int choice1TextLength;
    private string choice1TextTyping;
    private bool typingChoice2;
    private int choice2TextIndex;
    private int choice2TextLength;
    private string choice2TextTyping;
    private bool typingChoice3;
    private int choice3TextIndex;
    private int choice3TextLength;
    private string choice3TextTyping;
    private bool initialUpdate; //A flag that is true if the initial text update has been started
    private int typingDelayCounter; //Keeps track of how many frames it has been since a typing animation update
    public Animator optionAnimator;
    public Animator promptAnimator;
    
    private void Start()
    {
        //Updates all the text feilds and the manager
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<gameManagerScript>();
        choice1TextObject = GameObject.FindGameObjectWithTag("Choice1").GetComponent<TMP_Text>();
        choice2TextObject = GameObject.FindGameObjectWithTag("Choice2").GetComponent<TMP_Text>();
        choice3TextObject = GameObject.FindGameObjectWithTag("Choice3").GetComponent<TMP_Text>();
        promptTextObject = GameObject.FindGameObjectWithTag("Prompt").GetComponent<TMP_Text>();
        initialUpdate = false;
        typingDelayCounter = 0;
        OpenUI();
    }
    private void Update()
    {
        //Does an initial update when first loaded
        if (currentNode == 1 && prompts != null && !initialUpdate)
        {
            UpdateChoices();
            initialUpdate = true;
        }
        //Iterate through all the text to animate them on select frames based on the delay
        typingDelayCounter++;
        if (typingPrompt && typingDelayCounter >= typingDelay)
        {
            promptTextTyping = promptTextTyping + prompts[currentNode - 1][promptTextIndex];
            promptTextObject.text = promptTextTyping;
            promptTextIndex++;
            if (promptTextIndex == promptTextLength)
            {
                typingPrompt = false;
            }
        }
        if (typingChoice1 && typingDelayCounter >= typingDelay)
        {
            choice1TextTyping = choice1TextTyping + choice1Text[currentNode - 1][choice1TextIndex];
            choice1TextObject.text = choice1TextTyping;
            choice1TextIndex++;
            if (choice1TextIndex == choice1TextLength)
            {
                typingChoice1 = false;
            }
        }
        if (typingChoice2 && typingDelayCounter >= typingDelay)
        {
            choice2TextTyping = choice2TextTyping + choice2Text[currentNode - 1][choice2TextIndex];
            choice2TextObject.text = choice2TextTyping;
            choice2TextIndex++;
            if (choice2TextIndex == choice2TextLength)
            {
                typingChoice2 = false;
            }
        }
        if (typingChoice3 && typingDelayCounter >= typingDelay)
        {
            choice3TextTyping = choice3TextTyping + choice3Text[currentNode - 1][choice3TextIndex];
            choice3TextObject.text = choice3TextTyping;
            choice3TextIndex++;
            if (choice3TextIndex == choice3TextLength)
            {
                typingChoice3 = false;
            }
        }
        if (typingDelayCounter >= typingDelay)
        {
            typingDelayCounter = 0;
        }
    }
    public void HandleChoiceSelection(int selectedOption)
    {
        //This is called when a choice is selected and it update the current node or tells the game manager to change scene when the end is reached
        if (!(typingPrompt || typingChoice1 || typingChoice2 || typingChoice3))
        {
            switch (selectedOption)
            {
                case 0:
                    currentNode = choice1NextNode[currentNode - 1];
                    break;
                case 1:
                    currentNode = choice2NextNode[currentNode - 1];
                    break;
                case 2:
                    currentNode = choice3NextNode[currentNode - 1];
                    break;
                default:
                    break;
            }
        }
        if (choice1NextNode[currentNode - 1] == 0)
        {
            StartCoroutine(CloseUI("2"));
            ResolveChoiceResults();
        }
        else
        {
            UpdateChoices();
        }
    }

    public void SetUpChoices(string csvFilePath, int setTypingDelay, bool narrativeIncluded)
    {
        //Takes in the data from the csv and puts them in the relevant arrays
        currentNode = 1;
        typingDelay = setTypingDelay;
        var reader = new StreamReader(csvFilePath);
        reader.ReadLine(); //skip the titles
        while (!reader.EndOfStream)
        {
            var values = reader.ReadLine().Split(",");
            nodes.Add(Convert.ToInt32(values[0]));
            prompts.Add(values[1].Replace('|', ','));
            choice1Text.Add(values[2].Replace('|', ','));
            choice2Text.Add(values[3].Replace('|', ','));
            choice3Text.Add(values[4].Replace('|', ','));
            choice1NextNode.Add(Convert.ToInt32(values[5]));
            choice2NextNode.Add(Convert.ToInt32(values[6]));
            choice3NextNode.Add(Convert.ToInt32(values[7]));
            FinancialChanges.Add(Convert.ToInt32(values[8]));
            TeamMoralChanges.Add(Convert.ToInt32(values[9]));
            MoralityScore1Changes.Add(Convert.ToInt32(values[10]));
            MoralityScore2Changes.Add(Convert.ToInt32(values[11]));
            MoralityScore3Changes.Add(Convert.ToInt32(values[12]));
            MoralityScore4Changes.Add(Convert.ToInt32(values[13]));
            MoralityScore5Changes.Add(Convert.ToInt32(values[14]));
            MoralityScore6Changes.Add(Convert.ToInt32(values[15]));
        }
        /*
        if (!narrativeIncluded){
            Anonymise();
        }
        */
        //Sets the results start
        resultsStart = 0;
        while (prompts[resultsStart] != "")
        {
            resultsStart += 1;
        }
        
    }
    
    private void UpdateCharacterLists(List<string> listOfStrings){
        foreach (string text in listOfStrings){
            foreach (string name in unseenCharacters){
                if (!presentCharacters.Contains(name) && text.Contains(name)){
                    presentCharacters.Add(name);
                }
            }
        }
    }

    private void Anonymise(){
        UpdateCharacterLists(prompts);
        UpdateCharacterLists(choice1Text);
        UpdateCharacterLists(choice2Text);
        UpdateCharacterLists(choice3Text);
        for (int promptIndex = 0; promptIndex < prompts.Count(); promptIndex++){
            for(int presCharIndex = 0; presCharIndex < presentCharacters.Count(); presCharIndex++){
                prompts[promptIndex] = prompts[promptIndex].Replace(presentCharacters[presCharIndex], "Employee " + (presCharIndex + 1).ToString());
            }
            prompts[promptIndex] = prompts[promptIndex].Replace("he", "they");
            prompts[promptIndex] = prompts[promptIndex].Replace("He", "They");
            prompts[promptIndex] = prompts[promptIndex].Replace("she", "they");
            prompts[promptIndex] = prompts[promptIndex].Replace("She", "They");
            prompts[promptIndex] = prompts[promptIndex].Replace("him", "them");
            prompts[promptIndex] = prompts[promptIndex].Replace("Him", "Them");
            prompts[promptIndex] = prompts[promptIndex].Replace("her", "them");
            prompts[promptIndex] = prompts[promptIndex].Replace("Her", "Them");
            prompts[promptIndex] = prompts[promptIndex].Replace("his", "their");
            prompts[promptIndex] = prompts[promptIndex].Replace("His", "Their");
            prompts[promptIndex] = prompts[promptIndex].Replace("her", "their");
            prompts[promptIndex] = prompts[promptIndex].Replace("Her", "Their");
        }
        for (int choice1TextIndex = 0; choice1TextIndex < choice1Text.Count(); choice1TextIndex++){
            for(int presCharIndex = 0; presCharIndex < presentCharacters.Count(); presCharIndex++){
                choice1Text[choice1TextIndex] = choice1Text[choice1TextIndex].Replace(presentCharacters[presCharIndex], "Employee " + (presCharIndex + 1).ToString());
            }
            choice1Text[choice1TextIndex] = choice1Text[choice1TextIndex].Replace(" he ", " they ");
            choice1Text[choice1TextIndex] = choice1Text[choice1TextIndex].Replace("He", "They");
            choice1Text[choice1TextIndex] = choice1Text[choice1TextIndex].Replace(" she ", " they ");
            choice1Text[choice1TextIndex] = choice1Text[choice1TextIndex].Replace("She", "They");
            choice1Text[choice1TextIndex] = choice1Text[choice1TextIndex].Replace(" him ", " them ");
            choice1Text[choice1TextIndex] = choice1Text[choice1TextIndex].Replace("Him", "Them");
            choice1Text[choice1TextIndex] = choice1Text[choice1TextIndex].Replace(" her ", " them ");
            choice1Text[choice1TextIndex] = choice1Text[choice1TextIndex].Replace("Her", "Them");
            choice1Text[choice1TextIndex] = choice1Text[choice1TextIndex].Replace(" his ", " their ");
            choice1Text[choice1TextIndex] = choice1Text[choice1TextIndex].Replace("His", "Their");
            choice1Text[choice1TextIndex] = choice1Text[choice1TextIndex].Replace(" her ", " their ");
            choice1Text[choice1TextIndex] = choice1Text[choice1TextIndex].Replace("Her", "Their");
        }
        for (int choice2TextIndex = 0; choice2TextIndex < choice2Text.Count(); choice2TextIndex++){
            for(int presCharIndex = 0; presCharIndex < presentCharacters.Count(); presCharIndex++){
                choice2Text[choice2TextIndex] = choice2Text[choice2TextIndex].Replace(presentCharacters[presCharIndex], "Employee " + (presCharIndex + 1).ToString());
            }
            choice2Text[choice2TextIndex] = choice2Text[choice2TextIndex].Replace("he", "they");
            choice2Text[choice2TextIndex] = choice2Text[choice2TextIndex].Replace("He", "They");
            choice2Text[choice2TextIndex] = choice2Text[choice2TextIndex].Replace("she", "they");
            choice2Text[choice2TextIndex] = choice2Text[choice2TextIndex].Replace("She", "They");
            choice2Text[choice2TextIndex] = choice2Text[choice2TextIndex].Replace("him", "them");
            choice2Text[choice2TextIndex] = choice2Text[choice2TextIndex].Replace("Him", "Them");
            choice2Text[choice2TextIndex] = choice2Text[choice2TextIndex].Replace("her", "them");
            choice2Text[choice2TextIndex] = choice2Text[choice2TextIndex].Replace("Her", "Them");
            choice2Text[choice2TextIndex] = choice2Text[choice2TextIndex].Replace("his", "their");
            choice2Text[choice2TextIndex] = choice2Text[choice2TextIndex].Replace("His", "Their");
            choice2Text[choice2TextIndex] = choice2Text[choice2TextIndex].Replace("her", "their");
            choice2Text[choice2TextIndex] = choice2Text[choice2TextIndex].Replace("Her", "Their");
        }
        for (int choice3TextIndex = 0; choice3TextIndex < choice3Text.Count(); choice3TextIndex++){
            for(int presCharIndex = 0; presCharIndex < presentCharacters.Count(); presCharIndex++){
                choice3Text[choice3TextIndex] = choice3Text[choice3TextIndex].Replace(presentCharacters[presCharIndex], "Employee " + (presCharIndex + 1).ToString());
            }
            choice3Text[choice3TextIndex] = choice3Text[choice3TextIndex].Replace("he", "they");
            choice3Text[choice3TextIndex] = choice3Text[choice3TextIndex].Replace("He", "They");
            choice3Text[choice3TextIndex] = choice3Text[choice3TextIndex].Replace("she", "they");
            choice3Text[choice3TextIndex] = choice3Text[choice3TextIndex].Replace("She", "They");
            choice3Text[choice3TextIndex] = choice3Text[choice3TextIndex].Replace("him", "them");
            choice3Text[choice3TextIndex] = choice3Text[choice3TextIndex].Replace("Him", "Them");
            choice3Text[choice3TextIndex] = choice3Text[choice3TextIndex].Replace("her", "them");
            choice3Text[choice3TextIndex] = choice3Text[choice3TextIndex].Replace("Her", "Them");
            choice3Text[choice3TextIndex] = choice3Text[choice3TextIndex].Replace("his", "their");
            choice3Text[choice3TextIndex] = choice3Text[choice3TextIndex].Replace("His", "Their");
            choice3Text[choice3TextIndex] = choice3Text[choice3TextIndex].Replace("her", "their");
            choice3Text[choice3TextIndex] = choice3Text[choice3TextIndex].Replace("Her", "Their");
        }
        
    }
    

    private void UpdateChoices()
    {
        //Starts the animation if it has not already started otherwise skips the animation to the end
        if (typingPrompt || typingChoice1 || typingChoice2 || typingChoice3)
        {
            typingPrompt = false;
            typingChoice1 = false;
            typingChoice2 = false;
            typingChoice3 = false;
            promptTextObject.text = prompts[currentNode - 1];
            choice1TextObject.text = choice1Text[currentNode - 1];
            choice2TextObject.text = choice2Text[currentNode - 1];
            choice3TextObject.text = choice3Text[currentNode - 1];
        }
        else
        {
            promptTextIndex = 0;
            promptTextLength = prompts[currentNode - 1].Length;
            promptTextTyping = "";
            typingPrompt = true;
            choice1TextIndex = 0;
            choice1TextLength = choice1Text[currentNode - 1].Length;
            choice1TextTyping = "";
            typingChoice1 = true;
            choice2TextIndex = 0;
            choice2TextLength = choice2Text[currentNode - 1].Length;
            choice2TextTyping = "";
            typingChoice2 = true;
            choice3TextIndex = 0;
            choice3TextLength = choice3Text[currentNode - 1].Length;
            choice3TextTyping = "";
            typingChoice3 = true;
            typingDelayCounter = 10000;
        }
    }

    private void ResolveChoiceResults()
    {
        //Updates the score and tells the game manager to go to the results
        gameManager.UpdateScores(FinancialChanges[currentNode - 1], TeamMoralChanges[currentNode - 1], MoralityScore1Changes[currentNode - 1], MoralityScore2Changes[currentNode - 1], MoralityScore3Changes[currentNode - 1], MoralityScore4Changes[currentNode - 1], MoralityScore5Changes[currentNode - 1], MoralityScore6Changes[currentNode - 1]);
        gameManager.GoToResults(currentNode - resultsStart);
    }
    public void OpenUI()
    {
        optionAnimator.SetBool("isOpen", true);
        promptAnimator.SetBool("isOpen", true);
    }
    IEnumerator<WaitForSeconds> CloseUI(string time)
    {
        optionAnimator.SetBool("isOpen", false);
        promptAnimator.SetBool("isOpen", false);
        yield return new WaitForSeconds(int.Parse(time));
    }
}
