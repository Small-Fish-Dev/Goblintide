namespace GameJam;

public partial class Lord
{
	#region Player Configuration

	/// <summary> Player body height </summary>
	protected virtual float Height => 72.0f;

	protected virtual float EyeHeight => Height - 6.0f;

	/// <summary> Player eye position </summary>
	public Vector3 EyePosition => Position + Vector3.Up * EyeHeight;

	#endregion

	#region Camera Configuration

	/// <summary> Max distance from camera to player </summary>
	public float CameraDistance = 60f;

	private const float CameraRotationLerp = 15.0f;
	private const float MoveRotationLerp = 1.65f;
	private const float MoveCoolRotationLerp = 0.5f;
	private const float DistanceLerp = 15.0f;
	private static Vector3 PostOffset => Vector3.Up * 1 + Camera.Rotation.Right * 15f;

	private const float PitchBounds = 25.0f;

	#endregion

	#region Camera and Input Variables

	private Rotation _interimCameraRotation = Rotation.Identity;
	private float _proposedCameraDistance = 80.0f;
	private bool _isMovingBackwards;
	private bool _isMoving;

	/// <summary> Whether or not the player is holding the point button (RMB) </summary>
	[ClientInput]
	public bool Pointing { get; protected set; }

	/// <summary> Direction player should move </summary>
	[ClientInput]
	public Vector3 InputDirection { get; protected set; }

	private Angles _analogLook;

	#endregion

	/// <summary> Figure out where we want the camera to be </summary>
	private void CameraStageOne()
	{
		_interimCameraRotation *= _analogLook.WithRoll( 0 ).ToRotation();
		{
			var angles = _interimCameraRotation.Angles();
			angles.roll = 0;
			angles.pitch = float.Min( angles.pitch, PitchBounds );
			angles.pitch = float.Max( angles.pitch, -PitchBounds );

			_interimCameraRotation = angles.ToRotation();
		}

		if ( _isMoving && !_isMovingBackwards && _analogLook == Angles.Zero )
		{
			var proposedCameraRotation =
				_interimCameraRotation.Angles().WithYaw( InputDirection.EulerAngles.yaw ).ToRotation();

			var lerp = Time.Delta * (_analogLook == Angles.Zero ? MoveCoolRotationLerp : MoveRotationLerp);

			_interimCameraRotation = Rotation.Slerp( _interimCameraRotation, proposedCameraRotation,
				lerp );
		}
	}

	/// <summary> Figure out where the camera should be now </summary>
	private void CameraStageTwo()
	{
		{
			// Find camera rotation
			var proposedCameraRotation =
				Rotation.Slerp( Camera.Rotation, _interimCameraRotation, Time.Delta * CameraRotationLerp );
			{
				// Remove roll from lerped rotation
				var angles = proposedCameraRotation.Angles();
				angles.roll = 0;
				proposedCameraRotation = angles.ToRotation();
			}

			Camera.Rotation = proposedCameraRotation;
		}

		// Do a trace - get camera distance
		_proposedCameraDistance = _proposedCameraDistance.LerpTo( CameraDistance, Time.Delta * DistanceLerp );
		var trace = Trace.Ray( new Ray( EyePosition + PostOffset, Camera.Rotation.Backward ), _proposedCameraDistance )
			.Ignore( this )
			.WithoutTags( "player", "npc" )
			.Radius( 7 )
			.IncludeClientside()
			.Run();

		_proposedCameraDistance = trace.Distance;

		{
			// Find camera position
			var proposedCameraPosition = trace.EndPosition;

			Camera.Position = proposedCameraPosition;
		}

		// note(gio): stole the below from stud jump! teehee!
		var alpha = (Camera.Position.Distance( EyePosition ) / CameraDistance).Clamp( 0f, 1.1f ) - 0.1f;
		RenderColor = Color.White.WithAlpha( alpha );
		foreach ( var child in Children )
			if ( child is ModelEntity ent )
				ent.RenderColor = RenderColor;
	}

	/// <summary> Finish camera setup </summary>
	private void CameraFinalize()
	{
		Camera.FirstPersonViewer = null;
		Camera.ZNear = 4;
		Camera.FieldOfView = Screen.CreateVerticalFieldOfView( Game.Preferences.FieldOfView );
	}

	public void SimulateCamera()
	{
		CameraStageOne();
		CameraStageTwo();
		CameraFinalize();
	}

	public override void BuildInput()
	{
		_analogLook = Input.AnalogLook;
		var direction = Input.AnalogMove;

		_isMoving = direction.Length != 0.0f;
		_isMovingBackwards = direction.Normal.x < -0.8;

		InputDirection = direction.x * Camera.Rotation.Forward.Normal + -(direction.y * Camera.Rotation.Right.Normal);

		Pointing = Input.Down( InputButton.SecondaryAttack );

		DebugOverlay.ScreenText( $"AnalogLook: {Input.AnalogLook}", Vector2.One * 20, 0,
			Input.AnalogLook == Angles.Zero ? Color.Red : Color.Green );
		DebugOverlay.ScreenText( $"AnalogMove: {Input.AnalogMove}", Vector2.One * 20, 1,
			Input.AnalogMove.Length == 0 ? Color.Red : Color.Green );

		DebugOverlay.ScreenText( $"Camera Position: {Camera.Position}", Vector2.One * 20, 3, Color.Cyan );
		DebugOverlay.ScreenText( $"Camera Rotation: {Camera.Rotation}", Vector2.One * 20, 4, Color.Cyan );
		DebugOverlay.ScreenText( $"Interim Rotation: {_interimCameraRotation}", Vector2.One * 20, 5, Color.Cyan );
	}
}
