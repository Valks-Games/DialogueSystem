global using Godot;
global using System;
global using System.Collections.Generic;
global using System.Collections.Concurrent;
global using System.Diagnostics;
global using System.Runtime.CompilerServices;
global using System.Threading;
global using System.Text.RegularExpressions;
global using System.Threading.Tasks;
global using System.Linq;

namespace DialogueSystem;

public partial class GameManager : Node
{
	public static UIDialogue UIDialogue { get; private set; }

	public override void _Ready()
	{
		UIDialogue = GetNode<UIDialogue>("CanvasLayer/Dialogue");
	}
}
