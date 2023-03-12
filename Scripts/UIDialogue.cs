namespace DialogueSystem;

public class Dialogue
{
	public string Name { get; set; }
	public string Text { get; set; }
	public List<Choice> Choices { get; set; }
}

public class Choice
{
	public string Text { get; set; }
	public List<Dialogue> Dialogues { get; set; }
}

public partial class UIDialogue : Control
{
	private string CurConversation { get; set; } = "fairy_encounter_1";
	private int CurDialogIndex { get; set; }
	private List<Dialogue> CurDialogues { get; set; }
	private bool TrackInputDelay { get; set; }
	private bool DestroyAnimationActive { get; set; }

	// Anything lower than 2 is way too slow even for crazy plot dialogues
	[Export(PropertyHint.Range, "2, 100")] public double TextSpeed { get; set; } = 40.0;

	[Export] public NodePath NodePathActorName { get; set; }
	[Export] public NodePath NodePathActorDialogue { get; set; }
	[Export] public NodePath NodePathPanelName { get; set; }
	[Export] public NodePath NodePathPanelBack { get; set; }
	[Export] public NodePath NodePathPanelDialogue { get; set; }
	[Export] public NodePath NodePathSectionChoices { get; set; }

	private Sprite2D ActorPortrait { get; set; }
	private Label ActorName { get; set; }
	private Label ActorDialogue { get; set; }
	private Control PanelName { get; set; }
	private Control PanelBack { get; set; }
	private Control PanelDialogue { get; set; }
	private Control SectionChoices { get; set; }
	private GridContainer[] ChoiceRows { get; set; } = new GridContainer[2];
	private Button[] BtnChoices { get; set; } = new Button[4];

	private Vector2 PanelDialogueSize { get; set; }
	private Vector2 PanelBackPosition { get; set; }
	private Vector2 ActorPortraitStartingPos { get; set; }

	private PackedScene PrefabChoiceButton { get; set; } = GD.Load<PackedScene>("res://Scenes/Prefabs/BtnChoice.tscn");
	private bool ChoicesAreBeingShown { get; set; }

	private Tween TweenPanel { get; set; }
	private Tween TweenText { get; set; }
	private Tween TweenDelay { get; set; }

	public override void _Ready()
	{
		FileDialogues.ReadDialogues();

		ActorPortrait = GetNode<Sprite2D>("Portrait");
		ActorName = GetNode<Label>(NodePathActorName);
		ActorDialogue = GetNode<Label>(NodePathActorDialogue);
		PanelName = GetNode<Control>(NodePathPanelName);
		PanelBack = GetNode<Control>(NodePathPanelBack);
		PanelDialogue = GetNode<Control>(NodePathPanelDialogue);
		SectionChoices = GetNode<Control>(NodePathSectionChoices);
		ChoiceRows[0] = SectionChoices.GetNode<GridContainer>("Row1");
		ChoiceRows[1] = SectionChoices.GetNode<GridContainer>("Row2");

		CreateChoiceBtns();

		// Dialogue text should not make the panel grow above, only downwards
		PanelDialogue.GrowVertical = GrowDirection.End;

		// Get panel dialogue size and panel back position before setting the scale of panel dialogue to zero
		PanelDialogueSize = PanelDialogue.GetRect().Size;
		PanelBackPosition = PanelBack.GlobalPosition;
		//PanelDialogue.Scale = Vector2.Zero;

		ActorPortraitStartingPos = ActorPortrait.GlobalPosition;

		// Initially hide the dialogue panel
		Hide();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (ActorPortrait == null)
			return;

		var portraitPos = ActorPortrait.GlobalPosition;
		portraitPos.X = DisplayServer.WindowGetSize().X - ActorPortrait.GetRect().Size.X - 100;
		portraitPos.Y = PanelBack.GlobalPosition.Y - ActorPortrait.GetRect().Size.Y;
		//ActorPortrait.GlobalPosition = ActorPortrait.GlobalPosition.Lerp(portraitPos, 0.05f);
		ActorPortrait.GlobalPosition = portraitPos;
	}

	public override void _Input(InputEvent @event)
	{
		if (Input.IsActionJustPressed("player_skip_dialogue"))
		{
			if (ChoicesAreBeingShown || TrackInputDelay || DestroyAnimationActive)
				return;

			// UPDATE* DestroyAnimationActive bool fixed the below issue!

			// Prevent going to next dialogue for certain delay
			// If the delay is anything below 0.4 then ChoicesAreBeingShown will 
			// sometimes get stuck to true at the end of a conversation preventing
			// creating any further conversations for some unknown reason
			//TrackInputDelay = true;
			//GetTree().CreateTimer(0.4).Timeout += () => TrackInputDelay = false;

			var firstDialogue = false;

			if (!Visible)
			{
				firstDialogue = true;
				Reveal();

				// Prepare dialogue for first time
				CurDialogues = new List<Dialogue>(FileDialogues.Conversations[CurConversation]);
			}

			Show();
			NextDialogue(firstDialogue);
		}
	}

	private void CreateChoiceBtns()
	{
		var choiceButton1 = PrefabChoiceButton.Instantiate<Button>();
		BtnChoices[0] = choiceButton1;
		ChoiceRows[0].CallDeferred("add_child", choiceButton1);

		var choiceButton2 = PrefabChoiceButton.Instantiate<Button>();
		BtnChoices[1] = choiceButton2;
		ChoiceRows[0].CallDeferred("add_child", choiceButton2);

		var choiceButton3 = PrefabChoiceButton.Instantiate<Button>();
		BtnChoices[2] = choiceButton3;
		ChoiceRows[1].CallDeferred("add_child", choiceButton3);

		var choiceButton4 = PrefabChoiceButton.Instantiate<Button>();
		BtnChoices[3] = choiceButton4;
		ChoiceRows[1].CallDeferred("add_child", choiceButton4);
	}

	public void NextDialogue(bool firstDialogue)
	{
		// Hide the choices before each new dialogue is shown
		ChoicesAreBeingShown = false;
		HideChoices();

		// Are we at the end of the conversation?
		if (CurDialogIndex >= CurDialogues.Count)
		{
			// Close all the dialogue UIs
			CurDialogIndex = 0;
			ChoicesAreBeingShown = false;
			Destroy();
			return;
		}

		// Get the current dialog in the conversation
		var dialog = CurDialogues[CurDialogIndex++];

		// Display the dialogue
		Text(dialog.Name, dialog.Text, firstDialogue ? 0.5 : 0);

		// Are there choices to this dialogue?
		if (dialog.Choices == null || dialog.Choices.Count == 0)
		{
			// There are no choices in this dialogue
			ChoicesAreBeingShown = false;
			return;
		}

		// There are choices, lets show them
		SectionChoices.Show();
		ChoicesAreBeingShown = true;

		// Showing all choices that were defined
		for (int i = 0; i < BtnChoices.Length; i++)
			if (dialog.Choices.ElementAtOrDefault(i) != default(Choice))
			{
				var btn = BtnChoices[i];
				var choice = dialog.Choices[i];

				btn.Show();
				btn.Text = choice.Text; // Show the text for each choice
				btn.Pressed += onPressed;

				// Lets do something when we click on a choice
				void onPressed()
				{
					// On clicking a choice, all choices are no longer being shown
					// It does not matter if the choice leads to additional dialogue
					ChoicesAreBeingShown = false;

					var dialogues = choice.Dialogues;

					// Does this choice lead to additional dialogues?
					if (dialogues != null)
					{
						CurDialogues.InsertRange(CurDialogIndex, dialogues);
						NextDialogue(false);
					}
				}
			}
	}

	private void HideChoices()
	{
		SectionChoices.Hide();

		for (int i = 0; i < BtnChoices.Length; i++)
			BtnChoices[i].QueueFree();

		CreateChoiceBtns();

		for (int i = 0; i < BtnChoices.Length; i++)
			BtnChoices[i].Hide();
	}

	private void Reveal()
	{
		SetPhysicsProcess(true);
		Show();
		Visible = true;

		// ensure no other tween for panel is running
		TweenPanel?.Kill();

		AnimatePanel();
	}

	private void Text(string name, string dialogue, double delay = 0)
	{
		// ensure no other tween for text is running
		TweenText?.Kill();

		ActorName.Text = name;
		ActorDialogue.Text = dialogue;
		ActorDialogue.VisibleRatio = 0;

		AnimateText(delay);
	}

	public void Destroy()
	{
		DestroyAnimationActive = true;

		// exit animation for dialogue panel
		TweenPanel = GetTree().CreateTween();
		TweenPanel.TweenProperty(PanelDialogue, "scale", Vector2.Zero, 0.4);
		TweenPanel.TweenCallback(Callable.From(() => DestroyAnimationActive = false));

		// stop setting the NPC portrait position
		SetPhysicsProcess(false);

		// exit animation for NPC portrait
		var tween = GetTree().CreateTween();
		tween.TweenProperty(ActorPortrait, "position:y", DisplayServer.WindowGetSize().Y + ActorPortrait.GetRect().Size.Y, 0.4);
		tween.TweenCallback(Callable.From(() => Hide()));
	}

	private void AnimatePanel()
	{
		PanelDialogue.PivotOffset = PanelDialogueSize / 2;

		TweenPanel = GetTree().CreateTween();

		TweenPanel.TweenProperty(PanelDialogue, "scale", Vector2.One, 0.5)
			.From(Vector2.Zero)
			.SetTrans(Tween.TransitionType.Quad)
			.SetEase(Tween.EaseType.InOut);
	}

	private void AnimateText(double delay)
	{
		// allow 1 second for every 'TextSpeed' characters of text
		var duration = ActorDialogue.Text.Length / TextSpeed;

		TweenText = GetTree().CreateTween();
		TweenText.TweenProperty(ActorDialogue, "visible_ratio", 1, duration)
			.SetDelay(delay)
			.From(0.0);
	}
}
