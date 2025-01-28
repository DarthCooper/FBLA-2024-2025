using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class ScreenSpaceUIController : MonoBehaviour
{
    private EntityCommandBufferSystem commandBufferSystem;

    [Header("Health")]
    public Image playerHealthBar;

    [Header("Combat")]
    public Image meleeDelayVisual;
    public Image rangedDelayVisual;

    [Header("Dialogue")]
    public GameObject dialogueHolder;
    public Animator dialogueAnimator;
    public TMP_Text dialogueText;

    Dictionary<int, DialoguePos> runDialogues = new Dictionary<int, DialoguePos>();
    public Image DialogueTimer;
    public GameObject dialogueTimerText;

    public Image leftCharacter;
    public Image rightCharacter;

    [Header("Win Conditions")]
    public GameObject winConditionText;
    public GameObject winConditionName;
    public Transform winConditionHolder;
    Dictionary<int, (TMP_Text, TMP_Text)> winConditions = new Dictionary<int, (TMP_Text, TMP_Text)>();

    [Header("Choice Menu")]
    public GameObject choicePanel;
    public TMP_Text choiceDescription;
    public Button choice1Button;
    public Button choice2Button;

    private void Start()
    {
        commandBufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<EntityCommandBufferSystem>();
    }

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

        QuestSystem winSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<QuestSystem>();
        winSystem.QuestVisual += SetWinConditons;
        winSystem.OnEndQuest += DestroyWinCondition;

        ChoiceSystem choiceSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<ChoiceSystem>();
        choiceSystem.OnShowChoice += DisplayChoice;
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

        QuestSystem winSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<QuestSystem>();
        winSystem.QuestVisual -= SetWinConditons;
        winSystem.OnEndQuest -= DestroyWinCondition;
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

    public void ShowDialogue(string message, DialoguePos pos, int index, string leftSpritePath, string rightSpritePath)
    {
        if(runDialogues.ContainsKey(index)) { return; }
        if(!dialogueHolder.activeSelf) { dialogueHolder.SetActive(true); }
        dialogueText.text = message;
        dialogueTimerText.SetActive(false);

        var leftTexture = Instantiate(LoadPrefabFromFile(leftSpritePath), Vector3.zero, Quaternion.identity) as Texture2D;
        var rightTexture = Instantiate(LoadPrefabFromFile(rightSpritePath), Vector3.zero, Quaternion.identity) as Texture2D;

        leftCharacter.sprite = Sprite.Create(leftTexture, new Rect(0.0f, 0.0f, leftTexture.width, leftTexture.height), new Vector3(0.5f, 0.5f), 100.0f);
        rightCharacter.sprite = Sprite.Create(rightTexture, new Rect(0.0f, 0.0f, rightTexture.width, rightTexture.height), new Vector3(0.5f, 0.5f), 100.0f);

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
    }

    private UnityEngine.Object LoadPrefabFromFile(string filename)
    {
        Debug.Log("Trying to load LevelPrefab from file (" + filename + ")...");
        var loadedObject = Resources.Load("Sprites/" + filename);
        if (loadedObject == null)
        {
            throw new FileNotFoundException("...no file found - please check the configuration");
        }
        return loadedObject;
    }

    public void HideDialogue()
    {
        dialogueText.text = "";
        runDialogues.Clear();
        dialogueHolder.SetActive(false);
        leftCharacter.sprite = null;
        rightCharacter.sprite = null;
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

    public void SetWinConditons(string questName, string text, bool completed, int questID)
    {
        if(winConditions.ContainsKey(questID))
        {
            winConditions[questID].Item1.text = questName;
            winConditions[questID].Item2.text = text;
        }else
        {
            GameObject questNameObject = Instantiate(winConditionName, winConditionHolder);
            GameObject winTextObject = Instantiate(winConditionText, winConditionHolder);

            TMP_Text winText = winTextObject.GetComponent<TMP_Text>();
            TMP_Text questNameText = questNameObject.GetComponent<TMP_Text>();

            winText.text = text;
            winConditions.Add(questID, (questNameText, winText));
        }
    }

    public void DestroyWinCondition(int questID)
    {
        if(winConditions.ContainsKey(questID))
        {
            Destroy(winConditions[questID].Item1.gameObject);
            Destroy(winConditions[questID].Item2.gameObject);
            winConditions.Remove(questID);
        }
    }

    public void DisplayChoice(string description, string choice1text, string choice2text)
    {
        choice1Button.onClick.RemoveAllListeners();
        choice2Button.onClick.RemoveAllListeners();

        choicePanel.SetActive(true);
        choice1Button.GetComponentInChildren<TMP_Text>().text = choice1text;
        choice1Button.onClick.AddListener(Button1Clicked);

        choice2Button.GetComponentInChildren<TMP_Text>().text = choice2text;
        choice2Button.onClick.AddListener(Button2Clicked);
    }

    public void Button1Clicked()
    {
        var ecb = commandBufferSystem.CreateCommandBuffer();
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        EntityQuery query = entityManager.CreateEntityQuery(typeof(Choice));
        if(query.IsEmpty)
        {
            Debug.Log("No entities found");
            return;
        }
        var entities = query.ToEntityArray(Unity.Collections.Allocator.TempJob);
        var entity = entities[0];
        ecb.AddComponent<Button1Pressed>(entity);

        choicePanel.SetActive(false);
    }

    public void Button2Clicked()
    {
        var ecb = commandBufferSystem.CreateCommandBuffer();
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        EntityQuery query = entityManager.CreateEntityQuery(typeof(Choice));
        if (query.IsEmpty)
        {
            Debug.Log("No entities found");
            return;
        }
        var entities = query.ToEntityArray(Unity.Collections.Allocator.TempJob);
        var entity = entities[0];
        ecb.AddComponent<Button2Pressed>(entity);

        choicePanel.SetActive(false);
    }
}

public enum DialoguePos
{
    LEFT,
    RIGHT,
    NoOne,
}
