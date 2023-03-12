namespace DialogueSystem;

public class Conversation
{
	private string                         CurrentActorName     { get; set; } = "";
	private List<ActorDialogue> Dialogues            { get; set; } = new();
	private int                            CurrentDialogueIndex { get; set; }

	public void SetActor(string name) => CurrentActorName = name;

	public void AddDialogue(ActorDialogue dialogue) => Dialogues.Add(dialogue);
	public void AddDialogue(string text) => AddDialogue(CurrentActorName, text);
	public void AddDialogue(string actorName, string text) =>
		Dialogues.Add(new ActorDialogue { Name = actorName, Text = text });

	public ActorDialogue GetNextDialogue()
	{
		if (CurrentDialogueIndex > Dialogues.Count - 1)
		{
			CurrentDialogueIndex = 0; // auto reset dialogue index
			return null;
		}

		var dialogue = Dialogues[CurrentDialogueIndex];

		CurrentDialogueIndex++;
		
		return dialogue;
	}
}

public class ActorDialogue
{
	public string Name { get; set; }
	public string Text { get; set; }
	public List<ActorChoice> Choices { get; set; }
}

public class ActorChoice
{
	public string Text { get; set; } // e.g. of choice text "No, I will not!"
	public int CurDialogueIndex { get; set; }
	public List<ActorDialogue> Dialogues { get; set; } // choices can lead to other dialogues
}