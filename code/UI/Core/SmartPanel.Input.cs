using Sandbox.UI;

namespace GameJam;

public partial class SmartPanel<T> : Panel where T : Panel, new()
{
	public enum Action
	{
		Up, Down, Left, Right,
		Ok, Cancel
	}

	public static readonly int ActionCount = Enum.GetNames( typeof(Action) ).Length;

	public struct InputEvent
	{
		public readonly Action Action;
		public bool Pressed;
		public InputEvent( Action action ) => Action = action;

		public InputEvent WithPressed( bool value )
		{
			Pressed = value;
			return this;
		}

		public override string ToString() => $"InputEvent {Action}, Pressed {Pressed}";
	}

	#region s&box Panel Input -> PanelHost

	private struct InputTranslator<T>
	{
		public readonly T[] From;
		public readonly Action To;

		public InputTranslator( T[] from, Action to )
		{
			From = from;
			To = to;
		}
	}

	private static readonly InputTranslator<string>[] PanelInputToPanelHost =
	{
		new(new[] { "w", "up" }, Action.Up), new(new[] { "s", "down" }, Action.Down),
		new(new[] { "a", "left" }, Action.Left), new(new[] { "d", "right" }, Action.Right),
		new(new[] { "enter" }, Action.Ok), new(new[] { "escape" }, Action.Cancel),
	};

	public override void OnButtonEvent( ButtonEvent e )
	{
		base.OnButtonEvent( e );

		InputTranslator<string>? translatedInput = null;

		foreach ( var inputTranslator in PanelInputToPanelHost )
		{
			if ( !inputTranslator.From.Contains( e.Button ) )
				continue;

			translatedInput = inputTranslator;
			break;
		}

		if ( translatedInput == null )
			return;

		OnInputEvent( new InputEvent( translatedInput.Value.To ).WithPressed( e.Pressed ) );
	}

	#endregion

	#region s&box Client Input -> PanelHost

	private static readonly bool[] CiStatus = new bool[ActionCount];

	[Event.Client.BuildInput]
	private static void BuildInput()
	{
		var vec = Input.AnalogMove;
		const float threshold = 0.7f;

		var newStatus = new bool[ActionCount];
		switch ( vec.y )
		{
			case > threshold:
				newStatus[(int)Action.Left] = true;
				break;
			case < -threshold:
				newStatus[(int)Action.Right] = true;
				break;
		}

		switch ( vec.x )
		{
			case > threshold:
				newStatus[(int)Action.Up] = true;
				break;
			case < -threshold:
				newStatus[(int)Action.Down] = true;
				break;
		}

		if ( Input.Down( InputButton.Jump ) )
			newStatus[(int)Action.Ok] = true;
		if ( Input.Down( InputButton.Duck ) )
			newStatus[(int)Action.Cancel] = true;

		for ( var i = 0; i < newStatus.Length; i++ )
		{
			if ( newStatus[i] == CiStatus[i] )
				continue;

			Instance.OnInputEvent( new InputEvent( (Action)i ).WithPressed( newStatus[i] ) );

			CiStatus[i] = newStatus[i];
		}
	}

	#endregion

	protected virtual void OnInputEvent( InputEvent e ) { }
}
