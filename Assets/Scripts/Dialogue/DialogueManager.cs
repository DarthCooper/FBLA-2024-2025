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

