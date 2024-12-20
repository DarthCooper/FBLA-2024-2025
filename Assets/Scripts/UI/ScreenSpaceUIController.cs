using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class ScreenSpaceUIController : MonoBehaviour
{
    public Image playerHealthBar;

    public Image meleeDelayVisual;
    public Image rangedDelayVisual;

    public GameObject dialogueHolder;
    public Animator dialogueAnimator;
    public TMP_Text dialogueText;

    Dictionary<int, DialoguePos> runDialogues = new Dictionary<int, DialoguePos>();
    public Image DialogueTimer;
    public GameObject dialogueTimerText;

    private void OnEnable()
    {
        PlayerUISystem playerUISystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<PlayerUISystem>();
        playerUISystem.showHealth += ShowPlayerHealth;
        playerUISystem.showMeleeDelay += ShowMeleeDelay;
        playerUISystem.showRangedDelay += ShowRangeDelay;

        DialogueSystem dialogueSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<DialogueSystem>();
        dialogueSystem.OnTalk += ShowDialogue;
        dialogueSystem.OnTalkEnd += HideDialogue;
        dialogueSystem.OnDialogueCountdown += ShowTimer;
    }

    private void OnDisable()
    {
        if(World.DefaultGameObjectInjectionWorld == null) { return; }
        PlayerUISystem playerUISystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<PlayerUISystem>();
        playerUISystem.showHealth -= ShowPlayerHealth;
        playerUISystem.showMeleeDelay -= ShowMeleeDelay;
        playerUISystem.showRangedDelay -= ShowRangeDelay;

        DialogueSystem dialogueSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<DialogueSystem>();
        dialogueSystem.OnTalk -= ShowDialogue;
        dialogueSystem.OnTalkEnd -= HideDialogue;
        dialogueSystem.OnDialogueCountdown -= ShowTimer;
    }

    public void ShowPlayerHealth(float health, float maxHealth)
    {
        playerHealthBar.fillAmount = health / maxHealth;
    }

    public void ShowMeleeDelay(float delay, float maxDelay)
    {
        meleeDelayVisual.fillAmount = delay / maxDelay;
    }

    public void ShowRangeDelay(float delay, float maxDelay)
    {
        rangedDelayVisual.fillAmount = delay / maxDelay;
    }

    public void ShowDialogue(string message, DialoguePos pos, int index)
    {
        if(runDialogues.ContainsKey(index)) { return; }
        if(!dialogueHolder.activeSelf) { dialogueHolder.SetActive(true); }
        dialogueText.text = message;
        dialogueTimerText.SetActive(false);

        if (runDialogues.Count > 0)
        {
            if(!runDialogues.Last().Value.Equals(pos))
            {
                SetAnimation(pos);
            }
        }else
        {
            SetAnimation(pos);
        }

        runDialogues.Add(index, pos);
        Debug.Log(pos);
    }

    public void HideDialogue()
    {
        dialogueText.text = "";
        runDialogues.Clear();
        dialogueHolder.SetActive(false);
    }

    public void ShowTimer(float timer, float maxTime)
    {
        DialogueTimer.fillAmount = timer / maxTime;
        if(timer / maxTime >= 1)
        {
            dialogueTimerText.SetActive(true);
        }
    }

    public void SetAnimation(DialoguePos pos)
    {
        switch (pos)
        {
            case DialoguePos.LEFT:
                dialogueAnimator.SetTrigger("LeftSpeaking");
                break;
            case DialoguePos.RIGHT:
                dialogueAnimator.SetTrigger("RightSpeaking");
                break;
            case DialoguePos.NoOne:
                dialogueAnimator.SetTrigger("NoOneSpeaking");
                break;
            default:
                dialogueAnimator.SetTrigger("NoOneSpeaking");
                break;
        }
    }
}

public enum DialoguePos
{
    LEFT,
    RIGHT,
    NoOne,
}
