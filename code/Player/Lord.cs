﻿using static GameJam.WorldMapHost;

namespace GameJam;

public partial class Lord : BaseCharacter
{
	public static Lord Self => (Lord)Game.LocalPawn;

	public override float MaxHitPoints { get; set; } = 10f;

	public override FactionType DefaultFaction { get; set; } = FactionType.Goblins;

	public float BaseWalkSpeed { get; set; } = 140f;
	public float WalkSpeed { get; set; } = 140f;

	public override float CollisionWidth { get; set; } = 20f;
	public override float CollisionHeight { get; set; } = 40f;
	public override bool BlockNav { get; set; } = true;

	public override void Spawn()
	{
		base.Spawn();
		Faction = DefaultFaction;
		HitPoints = MaxHitPoints;

		SetModel( "models/goblin/goblin.vmdl" );
		SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, CollisionBox.Mins, CollisionBox.Maxs );

		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableTouch = true;

		Tags.Add( "Player" );
		Tags.Add( "Pushable" );
		Tags.Add( Faction.ToString() );

		// Add obscured glow.
		var glow = Components.GetOrCreate<Glow>();
		glow.Enabled = true;
		glow.ObscuredColor = Color.White;
		glow.Color = Color.Transparent;

		SetMaterialGroup( "old" );
		SetBodyGroup( "crown", 1 );
	}
	public override void Kill()
	{
		if ( GameMgr.State is not VillageState )
		{
			for ( int i = 0; i < GameMgr.GoblinArmy.Count; i++ )
				GameMgr.GoblinArmy.First()?.Delete();

			GameMgr.SetState<VillageState>();
		}

		HitPoints = MaxHitPoints;
		TotalAttackers = 0;
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		// Overview mode toggle.
		if ( GameMgr.State is VillageState )
		{
			if ( Input.Released( InputButton.Score ) )
			{
				Overview = !Overview;
				OverviewOffset = 0;
			}
		}
		else
		{
			Overview = false;
		}

		SimulateController();
		SimulateAnimations();
		SimulateCommanding();
		SimulateUpgrades();

		Scale = 1.2f; //Idk why it doesn't work on Spawn()
	}

	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );

		SimulateController(); // Smooth push
		SimulateCamera();
		SimulateAnimations();
	}

	[ConVar.Client( "gdbg_lord" )] private static bool ShowLordInfo { get; set; } = true;

	[Debug.Draw]
	private static void DebugDraw()
	{
		Debug.Section( "Lord", () =>
		{
			var lord = (Lord)Game.LocalPawn;
			Debug.Value( "Faction", lord.Faction );
			Debug.Value( "Hit Points", lord.HitPoints );
		}, ShowLordInfo );

		Debug.Section( "Lord Upgrades", () =>
		{
			var lord = (Lord)Game.LocalPawn;
			foreach ( var upgrade in lord.Upgrades ) Debug.Add( upgrade );
		} );
	}
}
