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
	public const float CameraDistance = 70f;

	private const float CameraRotationLerp = 20.0f;
	private const float CameraRotationPointingLerp = 35.0f;

	/// <summary> Lerp amount multiplier: used when following the player and the mouse is being moved </summary>
	private const float FollowRotationLerp = 1.65f;

	/// <summary> Lerp amount multiplier: used when following the player and a controller is being used </summary>
	private const float FollowControllerRotationLerp = 1.35f;

	/// <summary> Lerp amount multiplier: used when following the player and the mouse is still </summary>
	private const float FollowStillRotationLerp = 0.5f;

	private const float DistanceLerp = 15.0f;
	private const float PitchBounds = 25.0f;
	private const float PitchBoundsPointing = 65.0f;

	#endregion

	#region Camera and Input Variables

	private Rotation _interimCameraRotation = Rotation.Identity;
	private float _proposedCameraDistance = 60f;
	private float _lastTraceDistance;
	private Vector3 _cameraOffset;
	private bool _isMovingBackwards;
	private bool _isMoving;

	/// <summary> Whether or not the player is holding the point button (RMB) </summary>
	[ClientInput]
	public bool Pointing { get; protected set; }

	/// <summary> Direction player should move </summary>
	[ClientInput]
	public Vector3 InputDirection { get; protected set; }

	/// <summary>
	/// Direction player is looking
	/// note(gio): is this needed???
	/// </summary>
	[ClientInput]
	public Rotation LookDirection { get; protected set; }

	private Angles _analogLook;

	#endregion

	/// <summary> Whether or not the camera should follow the player movement </summary>
	private bool ShouldFollowMovement()
	{
		if ( !_isMoving )
			return false;
		if ( _isMovingBackwards )
			return false;
		if ( _analogLook != Angles.Zero )
			return false;
		if ( Pointing )
			return false;
		return true;
	}

	private float GetFollowLerpMultiplier()
	{
		if ( Input.UsingController )
			return FollowControllerRotationLerp;
		if ( _analogLook == Angles.Zero )
			return FollowStillRotationLerp;
		return FollowRotationLerp;
	}

	private Vector3 GetPostOffset()
	{
		if ( !Pointing )
			return Vector3.Up * 1 + Camera.Rotation.Right * 25f;
		return Vector3.Up * 1 + Camera.Rotation.Right * 25f;
	}

	private float GetTargetDistance()
	{
		if ( !Pointing )
			return CameraDistance;
		return CameraDistance / 2f;
	}

	private float GetCameraRotationLerp()
	{
		if ( !Pointing )
			return CameraRotationLerp;
		return CameraRotationPointingLerp;
	}

	private float GetPitchBounds()
	{
		if ( !Pointing )
			return PitchBounds;
		return PitchBoundsPointing;
	}

	/// <summary> Figure out where we want the camera to be </summary>
	private void CameraStageOne()
	{
		// Set camera distance		
		_interimCameraRotation *= _analogLook.WithRoll( 0 ).ToRotation();
		{
			var angles = _interimCameraRotation.Angles();
			angles.roll = 0;
			angles.pitch = float.Min( angles.pitch, GetPitchBounds() );
			angles.pitch = float.Max( angles.pitch, -GetPitchBounds() );

			_interimCameraRotation = angles.ToRotation();
		}

		if ( !ShouldFollowMovement() )
			return;

		var proposedCameraRotation =
			_interimCameraRotation.Angles().WithYaw( InputDirection.Normal.EulerAngles.yaw ).ToRotation();

		var lerp = Time.Delta * GetFollowLerpMultiplier();

		_interimCameraRotation = Rotation.Slerp( _interimCameraRotation, proposedCameraRotation,
			lerp );
	}

	/// <summary> Figure out where the camera should be now </summary>
	private void CameraStageTwo()
	{
		{
			// Find camera rotation
			var proposedCameraRotation =
				Rotation.Slerp( Camera.Rotation, _interimCameraRotation, Time.Delta * GetCameraRotationLerp() );
			{
				// Remove roll from lerped rotation
				var angles = proposedCameraRotation.Angles();
				angles.roll = 0;
				proposedCameraRotation = angles.ToRotation();
			}

			Camera.Rotation = proposedCameraRotation;
		}

		// Do a trace - get camera distance
		var targetDistance = GetTargetDistance();

		_cameraOffset = GetPostOffset(); // _cameraOffset.LerpTo( GetPostOffset(), Time.Delta * DistanceLerp );
		var trace = Trace.Ray( EyePosition, EyePosition + Camera.Rotation.Backward * targetDistance )
			.Ignore( this )
			.WithoutTags( "player", "npc" )
			.Radius( 7 )
			.IncludeClientside()
			.Run();

		{
			// Find camera position
			_lastTraceDistance = trace.Distance;
			_proposedCameraDistance = _proposedCameraDistance.LerpTo( MathF.Min( trace.Distance, targetDistance ),
				Time.Delta * DistanceLerp );
			var proposedCameraPosition = trace.StartPosition
			                             + trace.Direction * _proposedCameraDistance;

			Camera.Position = proposedCameraPosition + _cameraOffset;
		}

		// note(gio): stole the below from stud jump! teehee!
		if ( Pointing )
			RenderColor = Color.White.WithAlpha( 0.5f );
		else
			RenderColor =
				Color.White.WithAlpha( (Camera.Position.Distance( EyePosition ) / CameraDistance).Clamp( 0f, 1.1f ) -
				                       0.1f );
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

		LookDirection = Camera.Rotation;
	}

	[ConVar.Client( "gdbg_camera" )] private static bool ShowCameraInfo { get; set; } = true;

	[Debug.Draw]
	private static void DebugDraw()
	{
		Debug.Section( "Camera", () =>
		{
			var lord = (Lord)Game.LocalPawn;
			Debug.Value( "Position", Camera.Position );
			Debug.Value( "Rotation", Camera.Rotation );
			Debug.Value( "Rotation (interim)", lord._interimCameraRotation );
			Debug.Value( "Trace Distance", lord._lastTraceDistance );
			Debug.Value( "Proposed Distance", lord._proposedCameraDistance );
			Debug.Value( "Offset", lord._cameraOffset );
			Debug.Value( "Follow Lerp", lord.GetFollowLerpMultiplier() );
			Debug.Value( "Rotation Lerp", lord.GetCameraRotationLerp() );
		}, ShowCameraInfo );
	}
}
