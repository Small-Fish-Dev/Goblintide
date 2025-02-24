﻿using GoblinGame.UI;

namespace GoblinGame;

public partial class Lord
{
	[Net, Predicted] public bool Overview { get; set; } = false;

	public Vector3 OverviewOffset
	{
		get => overviewOffset;
		set
		{
			var town = Goblintide.CurrentTown;
			if ( town == null )
				return;

			overviewOffset = value.Normal * MathX.Clamp( value.Length, -town.TownRadius * 2, town.TownRadius * 2 );
		}
	}

	Vector3 overviewOffset;

	Vector3 pointOfInterest => Goblintide.CurrentTown != null
		? Goblintide.CurrentTown.Position
		: Vector3.Up * 512;

	#region Player Configuration

	/// <summary> Player body height </summary>
	protected virtual float Height => 54.0f;

	protected virtual float EyeHeight => Height - 6.0f;

	/// <summary> Player eye position </summary>
	public Vector3 EyePosition => Position + Vector3.Up * EyeHeight;

	#endregion

	#region Camera Configuration

	/// <summary> Max distance from camera to player </summary>
	public const float CameraDistance = 50f;

	private const float CameraRotationLerp = 20.0f;
	private const float CameraRotationPointingLerp = 35.0f;

	/// <summary> Lerp amount multiplier: used when following the player and the mouse is being moved </summary>
	private const float FollowRotationLerp = 1.65f;

	/// <summary> Lerp amount multiplier: used when following the player and a controller is being used </summary>
	private const float FollowControllerRotationLerp = 1.35f;

	/// <summary> Lerp amount multiplier: used when following the player and the mouse is still </summary>
	private const float FollowStillRotationLerp = 0.5f;

	private const float DistanceLerp = 15.0f;
	private const float PitchBounds = 40.0f;
	private const float PitchBoundsPointing = 65.0f;

	/// <summary> Delay after the last look input until the camera is followed </summary>
	private const float DelayBeforeFollow = 1.0f;

	#endregion

	#region Camera and Input Variables

	private Rotation _interimCameraRotation = Rotation.Identity;
	private float _proposedCameraDistance = 60f;
	private float _lastTraceDistance;
	private bool _isMovingBackwards;
	private bool _isMoving;
	private RealTimeUntil _followDelay;

	// Offsets
	// (+x == Right, +y == Up) 
	private Vector2 _proposedPostOffset;
	private Vector2 _currentPostOffset;

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
		if ( !_followDelay )
			return false;
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

	private void UpdatePostOffset()
	{
		_proposedPostOffset = new Vector2( 15, 1 );
	}

	private float GetTargetDistance()
	{
		if ( SkillTree.IsOpen )
			return CameraDistance * 1.4f;

		if ( WorldMap.IsOpen )
			return CameraDistance * 0.2f;
		
		if ( !Pointing )
			return CameraDistance;
		return CameraDistance / 2f;
	}

	private float GetCameraRotationLerp()
	{
		if ( SkillTree.IsOpen )
			return CameraRotationLerp * 0.6f;
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
		UpdatePostOffset();

		_currentPostOffset = Vector2.Lerp( _currentPostOffset, _proposedPostOffset, Time.Delta * DistanceLerp );

		// Set camera distance		
		_interimCameraRotation *= _analogLook.WithRoll( 0 ).ToRotation();
		{
			var angles = _interimCameraRotation.Angles();
			angles.roll = 0;
			angles.pitch = float.Min( angles.pitch, GetPitchBounds() );
			angles.pitch = float.Max( angles.pitch, -GetPitchBounds() );

			_interimCameraRotation = angles.ToRotation();
		}

		if ( SkillTree.IsOpen ) _interimCameraRotation = Rotation;

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
		_proposedCameraDistance = _proposedCameraDistance.LerpTo( GetTargetDistance(), Time.Delta * DistanceLerp );
		var trace = Trace.Ray( EyePosition, EyePosition + Camera.Rotation.Backward * _proposedCameraDistance )
			.Ignore( this )
			.WithoutTags( "player", "npc", "trigger", "nocamera" )
			.Radius( 7 )
			.IncludeClientside()
			.Run();

		_proposedCameraDistance = trace.Distance;
		_lastTraceDistance = trace.Distance;

		{
			// Find camera position
			var proposedCameraPosition = trace.StartPosition
			                             + trace.Direction * _proposedCameraDistance;

			Camera.Position = proposedCameraPosition;

			// Apply current camera offset
			Camera.Position += _currentPostOffset.x * Camera.Rotation.Right;
			Camera.Position += _currentPostOffset.y * Camera.Rotation.Up;
		}

		// note(gio): stole the below from stud jump! teehee!
		if ( SkillTree.IsOpen )
			RenderColor = Color.White.WithAlpha( 1.0f );
		else if ( Pointing )
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
		Camera.ZNear = 2;
		Camera.ZFar = 99999;
		Camera.FieldOfView = Screen.CreateVerticalFieldOfView( 90f );
	}

	public void SimulateCamera()
	{
		CameraEffects();
		CameraFinalize();

		if ( !Overview )
		{
			CameraStageOne();
			CameraStageTwo();

			return;
		}

		// Overview Camera
		var offset = Vector3.Up * 500f + Vector3.Backward * 250f;
		var targetPosition = pointOfInterest + OverviewOffset + offset;
		Camera.Position = Vector3.Lerp( Camera.Position, targetPosition, 5f * Time.Delta );
		Camera.Rotation = Rotation.LookAt( -offset );
	}

	public void CameraEffects()
	{
		var postProcess = Camera.Main.FindOrCreateHook<Sandbox.Effects.ScreenEffects>();

		postProcess.Vignette.Intensity = ( MaxHitPoints - HitPoints ) / MaxHitPoints + ( 0.5f - Math.Min( LastAttacked, 0.5f ) );
		postProcess.Vignette.Roundness = 1f;
		postProcess.Vignette.Smoothness = 1f;
		postProcess.Vignette.Color = Color.Red;
	}

	public override void BuildInput()
	{
		if ( SkillTree.IsOpen )
		{
			InputDirection = 0;
			_analogLook = Angles.Zero;
			Pointing = false;
			return;
		}

		_analogLook = Input.AnalogLook;
		var direction = Input.AnalogMove;

		_isMoving = direction.Length != 0.0f;
		_isMovingBackwards = direction.Normal.x < -0.8;

		if ( _analogLook != Angles.Zero ) _followDelay = DelayBeforeFollow;

		InputDirection = direction.x * Camera.Rotation.Forward.Normal + -(direction.y * Camera.Rotation.Right.Normal);

		Pointing = Input.Down( InputButton.SecondaryAttack );

		LookDirection = Camera.Rotation;
	}

	[ConVar.Client( "gdbg_camera" )] private static bool ShowCameraInfo { get; set; } = true;

	[Debug.Draw]
	private static void DebugCameraDraw()
	{
		Debug.Section( "Camera", () =>
		{
			var lord = (Lord)Game.LocalPawn;
			Debug.Value( "Position", Camera.Position );
			Debug.Value( "Rotation", Camera.Rotation );
			Debug.Value( "Follow Delay", $"{lord._followDelay.Relative:0.00}" );
			Debug.Value( "Rotation (interim)", lord._interimCameraRotation );
			Debug.Value( "Trace Distance", lord._lastTraceDistance );
			Debug.Value( "Proposed Distance", lord._proposedCameraDistance );
			Debug.Value( "Current Offset", lord._currentPostOffset );
			Debug.Value( "Proposed Offset", lord._proposedPostOffset );
			Debug.Value( "Follow Lerp", lord.GetFollowLerpMultiplier() );
			Debug.Value( "Rotation Lerp", lord.GetCameraRotationLerp() );
		}, ShowCameraInfo );
	}
}
