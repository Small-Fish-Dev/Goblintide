using GameJam.UI;
using Sandbox.Internal;
using static Sandbox.CitizenAnimationHelper;
using System.Collections.Generic;

namespace GameJam;

public enum RaidState
{
	Loading,
	Sneaking,
	Raiding,
	EndOfTimer,
	PostRaid
}

public partial class RaidingState : GameState
{

	RaidingHUD hud;
	static RaidingState instance;
	[Net, Change] RaidState currentState { get; set; } = RaidState.Loading;
	public RaidState CurrentState
	{
		get => currentState;
		set
		{
			OnCurrentStateChanged( currentState, value );
			currentState = value;
		}
	}
	[Net] public TimeUntil TimeBeforeRaidStart { get; set; }
	[Net] public TimeUntil TimeBeforeRaidEnds { get; set; }
	[Net] public TimeUntil TimeBeforeNextState { get; set; }

	public override void Initialize()
	{
		instance = this;

		if ( Game.IsClient )
		{
			hud = HUD.Instance.AddChild<RaidingHUD>();
			WorldMap.Delete();
			SkillTree.Delete();
			GameMgr.Music.Stop();
			GameMgr.Music = Sound.FromScreen( "sounds/music/evil_march.sound" );
			return;
		}

		GameMgr.GoblinArmyEnabled( false );
		GameMgr.PlaceGoblinArmy( false );
		GameMgr.Lord.Position = GameMgr.CurrentTown.Position + Vector3.Backward * ( GameMgr.CurrentTown.TownRadius + 400f );
		GameMgr.Lord.Rotation = Rotation.LookAt( GameMgr.CurrentTown.Position - GameMgr.Lord.Position );
	}

	public override void Changed( GameState state )
	{
		hud?.Delete( true );

		if ( Game.IsServer )
		{
			foreach ( var entity in Entity.All.OfType<BaseStructure>() )
				entity.Delete();

			Log.Info( "Autosaving..." );
			GameTask.RunInThreadAsync( async () =>
			{
				await GameMgr.GenerateSave( true );
				Log.Info( "Finished autosaving" );
			} );
		}
	}

	[Event.Tick.Server]
	private void onTick()
	{
		if ( GameMgr.Lord == null )
			return;

		// Block movement if in overview mode.
		GameMgr.Lord.BlockMovement = GameMgr.Lord.Overview || !GameMgr.CurrentTown.Generated;

		if ( GameMgr.CurrentTown.Generated && CurrentState == RaidState.Loading )
		{
			CurrentState = RaidState.Sneaking;
		}

		if ( CurrentState == RaidState.Sneaking )
		{
			Log.Error( $"Sneaking time! ({TimeBeforeRaidStart.Relative}s left)" );

			if ( TimeBeforeRaidStart )
				CurrentState = RaidState.Raiding;
		}

		if ( CurrentState == RaidState.Raiding )
		{
			Log.Error( $"Raiding time! ({TimeBeforeRaidEnds.Relative}s left)" );

			if ( TimeBeforeRaidEnds )
				CurrentState = RaidState.EndOfTimer;
		}

		if ( CurrentState == RaidState.EndOfTimer )
		{
			var clearingDistance = GameMgr.CurrentTown.ForestRadius;

			if ( Time.Tick % 15 == 0 )
			{
				var distance = Game.Random.Float( clearingDistance - 100f, clearingDistance + 100f );
				var newPosition = GameMgr.CurrentTown.Position + Vector3.Random.WithZ( 0 ).Normal * distance;

				var spawnedEntity = BaseNPC.FromPrefab( "prefabs/npcs/soldier.prefab" );
				if ( spawnedEntity != null )
				{
					spawnedEntity.Position = newPosition;

					Town.TownEntities.Add( spawnedEntity );
				}
			}

			Log.Error( $"Escape to the forest! The guards are coming!" );

			
		}

		if ( GameMgr.Lord.Position.Distance( GameMgr.CurrentTown.Position ) >= GameMgr.CurrentTown.ForestRadius && CurrentState != RaidState.PostRaid )
		{
			CurrentState = RaidState.PostRaid;
			TimeBeforeNextState = 0.2f;
		}

		if ( CurrentState == RaidState.PostRaid )
			if ( TimeBeforeNextState )
				GameMgr.SetState<VillageState>();

	}

	public void OnCurrentStateChanged( RaidState oldState, RaidState newState ) // Run on server
	{
		if ( newState == RaidState.Sneaking )
			TimeBeforeRaidStart = 15f;

		if ( newState == RaidState.Raiding )
		{
			TimeBeforeRaidEnds = (int)Math.Sqrt( GameMgr.CurrentTown.TownSize * 100 );
			GameMgr.GoblinArmyEnabled( true );
		}

	}

	public void OncurrentStateChanged( RaidState oldState, RaidState newState ) // RUNS ON CLIENT
	{

		if ( newState == RaidState.Sneaking )
		{
			GameMgr.Music.Stop();
			GameMgr.Music = Sound.FromScreen( "sounds/music/exotic_battle.sound" );
		}

		if ( newState == RaidState.Raiding )
		{
			GameMgr.Music.Stop();
			GameMgr.Music = Sound.FromScreen( "sounds/music/failing_defense.sound" );
			Sound.FromScreen( "sounds/ui/trumpets_start.sound" );
		}

		if ( newState == RaidState.PostRaid )
		{
			GameMgr.Music.Stop();
			Sound.FromScreen( "sounds/ui/trumpets_fanfare.sound" );
		}
	}
}
