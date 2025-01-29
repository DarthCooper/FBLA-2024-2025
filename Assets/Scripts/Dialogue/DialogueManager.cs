using System.Collections.Generic;

public class DialogueManager
{
    public static List<string> completedDialogues = new List<string>();

    public static void CompleteDialogue(string dialogueKey)
    {
        completedDialogues.Add(dialogueKey);
    }

    public static bool DialogueComplete(string dialogueKey)
    {
        return completedDialogues.Contains(dialogueKey);
    }
}

public class QuestManager
{
    public static List<int> completedQuests = new List<int>();

    public static void CompleteQuest(int id)
    {
        completedQuests.Add(id);
    }

    public static bool QuestComplete(int id)
    {
        return completedQuests.Contains(id);
    }
}

