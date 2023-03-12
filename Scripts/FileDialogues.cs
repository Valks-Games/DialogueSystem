namespace DialogueSystem;

public class FileDialogues
{
	public static Dictionary<string, List<Dialogue>> Conversations { get; set; } = new();
	private static string CurrentConversation { get; set; }
	private static int CurrentChoice { get; set; }
	private static int CurrentDialog { get; set; }
	private static int ReadChoiceDialogues { get; set; }
	private static Dictionary<int, Choice> CurrentChoices { get; set; } = new();
	private static bool ReadingChoiceDialogue { get; set; }

	public static void ReadDialogues()
	{
		using var dir = DirAccess.Open("res://Dialogues");

		foreach (var fileName in dir.GetFiles())
		{
			CurrentConversation = fileName.Replace(".txt", "");
			Conversations.Add(CurrentConversation, new List<Dialogue>());

			using var file = FileAccess.Open($"res://Dialogues/{fileName}", FileAccess.ModeFlags.Read);

			while (!file.EofReached())
			{
				var line = file.GetLine();

				var star = line.IndexOf('*');
				var colon = line.IndexOf(':');
				var openSquareBracket = line.IndexOf('[');

				if (openSquareBracket != -1)
				{
					ReadChoiceBracketsStart(line);
					continue;
				}

				if (star != -1)
				{
					ReadChoice(line);
					continue;
				}

				if (colon != -1)
				{
					ReadDialogue(line, colon);
					continue;
				}
			}
		}
	}

	private static void ReadChoiceBracketsStart(string line)
	{
		var closedSquareBracket = line.IndexOf(']');
		var contents = line.Substring(1, closedSquareBracket - 1);

		if (contents.Contains("choice"))
		{
			ReadingChoiceDialogue = true;

			var choiceNum = int.Parse(line.Substring(closedSquareBracket - 1, 1));
			CurrentChoice = choiceNum;
		}

		if (contents.Contains("end"))
		{
			var choices = Conversations[CurrentConversation][CurrentDialog].Choices;

			choices.Add(CurrentChoices[CurrentChoice]);

			ReadingChoiceDialogue = false;
			ReadChoiceDialogues++;

			if (ReadChoiceDialogues >= CurrentChoices.Count)
			{
				CurrentDialog++;
				CurrentChoice = 0; // just added this line
				ReadChoiceDialogues = 0;
				CurrentChoices.Clear();
			}
		}
	}

	private static void ReadChoice(string line)
	{
		var colon = line.IndexOf(':');

		var choiceNum = int.Parse(line.Substring(colon - 1, 1));
		var text = line.Substring(colon + 2);

		CurrentChoices[choiceNum] = new Choice
		{
			Text = text,
			Dialogues = new List<Dialogue>()
		};
	}

	private static void ReadDialogue(string line, int colon)
	{
		var name = line.Substring(0, colon);
		var text = line.Substring(colon + 2);

		if (ReadingChoiceDialogue)
		{
			CurrentChoices[CurrentChoice].Dialogues.Add(new Dialogue
			{
				Name = name,
				Text = text,
				Choices = new List<Choice>()
			});
		}
		else
		{
			Conversations[CurrentConversation].Add(new Dialogue
			{
				Name = name,
				Text = text,
				Choices = new List<Choice>()
			});
		}
	}
}
