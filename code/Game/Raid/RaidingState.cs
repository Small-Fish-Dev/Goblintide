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
	public static RaidingState instance; // TODO: shouldnt really be public
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
		instance?.hud?.Delete( true );
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
		instance?.hud?.Delete( true );

		if ( Game.IsServer )
		{
			GameMgr.Tutorial = false;
			foreach ( var entity in Entity.All.OfType<BaseStructure>() )
				entity.Delete();

			Log.Info( "Autosaving..." );
			GameMgr.GenerateSave( true );
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
			if ( TimeBeforeRaidStart )
				CurrentState = RaidState.Raiding;
		}

		if ( CurrentState == RaidState.Raiding )
		{
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

			
		}

		if ( GameMgr.Lord.Position.Distance( GameMgr.CurrentTown.Position ) >= GameMgr.CurrentTown.ForestRadius - 200f && CurrentState != RaidState.PostRaid )
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
		{
			TimeBeforeRaidStart = 15f;
			if ( GameMgr.Tutorial )
			{
				GameMgr.Instance.tutorialPhrases.Add( "Sneak around and take note of the loot in this village! Your army is coming soon." );
				GameMgr.Instance.nextTutorial = 0.5f;
			}
		}

		if ( newState == RaidState.Raiding )
		{
			TimeBeforeRaidEnds = (int)Math.Sqrt( GameMgr.CurrentTown.TownSize * 100 ) * 2f;
			GameMgr.GoblinArmyEnabled( true );
			if ( GameMgr.Tutorial )
			{
				GameMgr.Instance.tutorialPhrases.Add( "The goblins are coming! Direct them towards enemies and loot!" );
				GameMgr.Instance.tutorialPhrases.Add( "Hold Right Click to point and Left Click to command nearby goblins." );
				GameMgr.Instance.tutorialPhrases.Add( "Make sure not to leave anything behind!" );
				GameMgr.Instance.nextTutorial = 0.5f;
			}
		}

		if ( newState == RaidState.EndOfTimer )
		{
			if ( GameMgr.Tutorial )
			{
				GameMgr.Instance.tutorialPhrases.Add( "You took too long, the guards are coming! Escape into the forest to return to base." );
				GameMgr.Instance.nextTutorial = 0.5f;
			}
		}

	}

	public void OncurrentStateChanged( RaidState oldState, RaidState newState ) // RUNS ON CLIENT
	{

		if ( newState == RaidState.Sneaking )
		{
			GameMgr.Music.Stop();
			GameMgr.Music = Sound.FromScreen( "sounds/music/exotic_battle.sound" );
			RaidingHUD.ShowLoading( false );
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
