namespace DialogueSystem;

public partial class Dialogue : Control
{
	// Anything lower than 2 is way too slow even for crazy plot dialogues
	[Export(PropertyHint.Range, "2, 100")] public double TextSpeed { get; set; } = 40.0;

	[Export] public NodePath NodePathActorName      { get; set; }
	[Export] public NodePath NodePathActorDialogue  { get; set; }
	[Export] public NodePath NodePathPanelName      { get; set; }
	[Export] public NodePath NodePathPanelBack      { get; set; }
	[Export] public NodePath NodePathPanelDialogue  { get; set; }
	[Export] public NodePath NodePathSectionChoices { get; set; }

	private Sprite2D      ActorPortrait  { get; set; }
	private Label         ActorName      { get; set; }
	private Label         ActorDialogue  { get; set; }
	private Control       PanelName      { get; set; }
	private Control       PanelBack      { get; set; }
	private Control       PanelDialogue  { get; set; }
	private Control       SectionChoices { get; set; }
	private GridContainer ChoicesRow1    { get; set; }
	private GridContainer ChoicesRow2    { get; set; }
	private Button        BtnChoice1     { get; set; }
	private Button        BtnChoice2     { get; set; }
	private Button        BtnChoice3     { get; set; }
	private Button        BtnChoice4     { get; set; }

	private Vector2 PanelDialogueSize        { get; set; }
	private Vector2 PanelBackPosition        { get; set; }
	private Vector2 ActorPortraitStartingPos { get; set; }

	private Tween TweenPanel { get; set; }
	private Tween TweenText  { get; set; }

	private Dictionary<string, Conversation> Conversations { get; set; } = new();
	private Conversation C { get; set; } // example conversation

	private Conversation CurrentConversation { get; set; }

	public override void _Ready()
	{
		// temporary spot for creating these conversations
		C = new Conversation();
		C.SetActor("Minnie");
		C.AddDialogue("Where did everyone go?");
		C.AddDialogue("Am I the only one here?");
		C.AddDialogue("I'll start looking over here first");
		C.AddDialogue("Maybe I'll find someone that can tell me what's going on here");

		C.AddDialogue(new ActorDialogue 
			{ 
				Name = "Fairy", Text = "Hello",
				Choices = new List<ActorChoice> 
				{
					{ new ActorChoice
						{
							Text = "Woah who are you?",
							Dialogues = new List<ActorDialogue>
							{
								{ 
									new ActorDialogue 
									{
										Name = "Fairy", Text = "I'm a fairy of course!"
									}
								},
								{
									new ActorDialogue
									{
										Name = "Fairy", Text = "Want to see something cool?"
									}
								}
							}
						}
					}	
				}
			});

		C.AddDialogue("Fairy", "*disappears*");

		Conversations.Add("fairy_encounter_1", C);

		CurrentConversation = C;

		ActorPortrait  = GetNode<Sprite2D>("Portrait");
		ActorName      = GetNode<Label>   (NodePathActorName);
		ActorDialogue  = GetNode<Label>   (NodePathActorDialogue);
		PanelName      = GetNode<Control> (NodePathPanelName);
		PanelBack      = GetNode<Control> (NodePathPanelBack);
		PanelDialogue  = GetNode<Control> (NodePathPanelDialogue);
		SectionChoices = GetNode<Control> (NodePathSectionChoices);
		ChoicesRow1    = SectionChoices.GetNode<GridContainer>("Row1");
		ChoicesRow2    = SectionChoices.GetNode<GridContainer>("Row2");
		BtnChoice1     = ChoicesRow1.GetNode<Button>("Choice1");
		BtnChoice2     = ChoicesRow1.GetNode<Button>("Choice2");
		BtnChoice3     = ChoicesRow2.GetNode<Button>("Choice3");
		BtnChoice4     = ChoicesRow2.GetNode<Button>("Choice4");

		// Dialogue text should not make the panel grow above, only downwards
		PanelDialogue.GrowVertical = GrowDirection.End;

		// Get panel dialogue size and panel back position before setting the scale of panel dialogue to zero
		PanelDialogueSize = PanelDialogue.GetRect().Size;
		PanelBackPosition = PanelBack.GlobalPosition;
		PanelDialogue.Scale = Vector2.Zero;

		ActorPortraitStartingPos = ActorPortrait.GlobalPosition;

		// Initially hide the dialogue panel
		Destroy();
	}

	public override void _PhysicsProcess(double delta)
	{
		var portraitPos = ActorPortrait.GlobalPosition;
		portraitPos.Y = PanelBack.GlobalPosition.Y - ActorPortrait.GetRect().Size.Y;
		ActorPortrait.GlobalPosition = ActorPortrait.GlobalPosition.Lerp(portraitPos, 0.05f);
	}

	public override void _Input(InputEvent @event)
	{
		if (Input.IsActionJustPressed("player_skip_dialogue"))
		{
			var firstDialogue = false;

			if (!Visible)
			{
				firstDialogue = true;
				Reveal();
			}

			NextDialogue(firstDialogue);
		}
	}

	public void NextDialogue(bool firstDialogue)
	{
		var dialogue = CurrentConversation.GetNextDialogue();

		if (dialogue == null) // no more dialogue
		{
			Destroy();
			return;
		}

		SectionChoices.Hide();
		BtnChoice1.Hide();
		BtnChoice2.Hide();
		BtnChoice3.Hide();
		BtnChoice4.Hide();

		// Are there any choices for this dialogue?
		if (dialogue.Choices != null && dialogue.Choices.Count != 0)
		{
			SectionChoices.Show();

			var row1Choices = 0;
			var row2Choices = 0;

			DialogueChoice(dialogue, 0, BtnChoice1, ref row1Choices);
			DialogueChoice(dialogue, 1, BtnChoice2, ref row1Choices);

			ChoicesRow1.Columns = row1Choices == 0 ? 1 : row1Choices;

			DialogueChoice(dialogue, 2, BtnChoice3, ref row2Choices);
			DialogueChoice(dialogue, 3, BtnChoice4, ref row2Choices);

			ChoicesRow2.Columns = row2Choices == 0 ? 1 : row2Choices;
		}
		else
			SectionChoices.Hide();

		Text(dialogue.Name, dialogue.Text, firstDialogue ? 0.5 : 0);
	}

	private void DialogueChoice(ActorDialogue dialogue, int choiceIndex, Button btn, ref int rowChoices)
	{
		var choice = dialogue.Choices.ElementAtOrDefault(choiceIndex);

		if (choice != null)
		{
			btn.Show();
			btn.Text = choice.Text;

			btn.Pressed += () => 
			{
				var dialogue = choice.Dialogues[0];

				// copy pasted this here (TEMPORARY)
				SectionChoices.Hide();
				BtnChoice1.Hide();
				BtnChoice2.Hide();
				BtnChoice3.Hide();
				BtnChoice4.Hide();

				Text(dialogue.Name, dialogue.Text);
			};

			rowChoices++;
		}
	}

	private void Reveal()
	{
		SetPhysicsProcess(true);
		Show();

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
		// exit animation for dialogue panel
		TweenPanel = GetTree().CreateTween();
		TweenPanel.TweenProperty(PanelDialogue, "scale", Vector2.Zero, 0.4);

		// stop setting the NPC portrait position
		SetPhysicsProcess(false);

		// exit animation for NPC portrait
		var tween = GetTree().CreateTween();
		tween.TweenProperty(ActorPortrait, "position:y", ActorPortraitStartingPos.Y, 0.4);
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
