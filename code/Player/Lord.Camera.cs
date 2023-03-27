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
	public float CameraDistance { get; protected set; } = 84f;

	private const float CameraRotationLerp = 15.0f;
	private const float DistanceLerp = 15.0f;
	private static readonly Vector3 PostOffset = Vector3.Up * 3;

	#endregion

	#region Camera Variables

	private Rotation _interimCameraRotation = Rotation.Identity;
	private float _proposedCameraDistance = 80.0f;

	#endregion

	/// <summary> Figure out where we want the camera to be </summary>
	private void CameraStageOne()
	{
		_interimCameraRotation *= AnalogLook.WithRoll( 0 ).ToRotation();
		var angles = _interimCameraRotation.Angles();
		angles.roll = 0;
		_interimCameraRotation = angles.ToRotation();
	}

	/// <summary> Figure out where the camera should be now </summary>
	private void CameraStageTwo()
	{
		var newCameraRotation =
			Rotation.Slerp( Camera.Rotation, _interimCameraRotation, Time.Delta * CameraRotationLerp );
		{
			// Remove roll from lerped rotation
			var angles = newCameraRotation.Angles();
			angles.roll = 0;
			newCameraRotation = angles.ToRotation();
		}

		_proposedCameraDistance = _proposedCameraDistance.LerpTo( CameraDistance, Time.Delta * DistanceLerp );
		var trace = Trace.Ray( new Ray( EyePosition, Camera.Rotation.Backward ), _proposedCameraDistance )
			.Ignore( this )
			.WithoutTags( "player" )
			.Radius( 7 )
			.IncludeClientside()
			.Run();

		_proposedCameraDistance = trace.Distance;

		Camera.Position = trace.EndPosition + PostOffset;
		Camera.Rotation = newCameraRotation;
		Camera.FirstPersonViewer = null;
		Camera.ZNear = 4;

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
		Camera.FieldOfView = Screen.CreateVerticalFieldOfView( Game.Preferences.FieldOfView );
	}

	public void SimulateCamera()
	{
		CameraStageOne();
		CameraStageTwo();
		CameraFinalize();
	}

	[ClientInput] public Vector3 InputDirection { get; protected set; }
	private Angles AnalogLook;

	public override void BuildInput()
	{
		AnalogLook = Input.AnalogLook;
		var direction = Input.AnalogMove;
		InputDirection = direction.x * Camera.Rotation.Forward.Normal + -(direction.y * Camera.Rotation.Right.Normal);
	}
}
