using GoblinGame.UI;
using Sandbox.Internal;
using static Sandbox.CitizenAnimationHelper;
using System.Collections.Generic;

namespace GoblinGame;

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
			Goblintide.Music.Stop();
			Goblintide.Music = Sound.FromScreen( "sounds/music/evil_march.sound" );
			return;
		}

		Goblintide.GoblinArmyEnabled( false );
		Goblintide.PlaceGoblinArmy( false );
		Goblintide.Lord.Position = Goblintide.CurrentTown.Position + Vector3.Backward * ( Goblintide.CurrentTown.TownRadius + 400f );
		Goblintide.Lord.Rotation = Rotation.LookAt( Goblintide.CurrentTown.Position - Goblintide.Lord.Position );
	}

	public override void Changed( GameState state )
	{
		instance?.hud?.Delete( true );

		if ( Game.IsServer )
		{
			Goblintide.Tutorial = false;

			Log.Info( "Autosaving..." );
			Goblintide.GenerateSave( true );
		}
	}

	[Event.Tick.Server]
	private void onTick()
	{
		if ( Goblintide.Lord == null )
			return;

		// Block movement if in overview mode.
		Goblintide.Lord.BlockMovement = Goblintide.Lord.Overview || !Goblintide.CurrentTown.Generated;

		if ( Goblintide.CurrentTown.Generated && CurrentState == RaidState.Loading )
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
			var clearingDistance = Goblintide.CurrentTown.ForestRadius;

			if ( Time.Tick % 15 == 0 )
			{
				var distance = Game.Random.Float( clearingDistance - 100f, clearingDistance + 100f );
				var newPosition = Goblintide.CurrentTown.Position + Vector3.Random.WithZ( 0 ).Normal * distance;

				var spawnedEntity = BaseNPC.FromPrefab( "prefabs/npcs/soldier.prefab" );
				if ( spawnedEntity != null )
				{
					spawnedEntity.Position = newPosition;

					Town.TownEntities.Add( spawnedEntity );
				}
			}

			
		}

		if ( Goblintide.Lord.Position.Distance( Goblintide.CurrentTown.Position ) >= Goblintide.CurrentTown.ForestRadius - 200f && CurrentState != RaidState.PostRaid )
		{
			CurrentState = RaidState.PostRaid;
			TimeBeforeNextState = 0.2f;
		}

		if ( CurrentState == RaidState.PostRaid )
			if ( TimeBeforeNextState )
				Goblintide.SetState<VillageState>();

	}

	public void OnCurrentStateChanged( RaidState oldState, RaidState newState ) // Run on server
	{
		if ( newState == RaidState.Sneaking )
		{
			TimeBeforeRaidStart = 15f;
			if ( Goblintide.Tutorial )
			{
				Goblintide.Instance.tutorialPhrases.Add( "Sneak around and take note of the loot in this village! Your army is coming soon." );
				Goblintide.Instance.nextTutorial = 0.5f;
			}
		}

		if ( newState == RaidState.Raiding )
		{
			TimeBeforeRaidEnds = (int)Math.Sqrt( Goblintide.CurrentTown.TownSize * 100 ) * 2f;
			Goblintide.GoblinArmyEnabled( true );
			if ( Goblintide.Tutorial )
			{
				Goblintide.Instance.tutorialPhrases.Add( "The goblins are coming! Direct them towards enemies and loot!" );
				Goblintide.Instance.tutorialPhrases.Add( "Hold Right Click to point and Left Click to command nearby goblins." );
				Goblintide.Instance.tutorialPhrases.Add( "Make sure not to leave anything behind!" );
				Goblintide.Instance.nextTutorial = 0.5f;
			}
		}

		if ( newState == RaidState.EndOfTimer )
		{
			if ( Goblintide.Tutorial )
			{
				Goblintide.Instance.tutorialPhrases.Add( "You took too long, the guards are coming! Escape into the forest to return to base." );
				Goblintide.Instance.nextTutorial = 0.5f;
			}
		}

	}

	public void OncurrentStateChanged( RaidState oldState, RaidState newState ) // RUNS ON CLIENT
	{

		if ( newState == RaidState.Sneaking )
		{
			Goblintide.Music.Stop();
			Goblintide.Music = Sound.FromScreen( "sounds/music/exotic_battle.sound" );
			RaidingHUD.ShowLoading( false );
		}

		if ( newState == RaidState.Raiding )
		{
			Goblintide.Music.Stop();
			Goblintide.Music = Sound.FromScreen( "sounds/music/failing_defense.sound" );
			Sound.FromScreen( "sounds/ui/trumpets_start.sound" );
		}

		if ( newState == RaidState.PostRaid )
		{
			Goblintide.Music.Stop();
			Sound.FromScreen( "sounds/ui/trumpets_fanfare.sound" );
		}
	}
}
