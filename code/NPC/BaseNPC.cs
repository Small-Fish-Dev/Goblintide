﻿namespace GameJam;

[Prefab, Category( "NPC" )]
public partial class BaseNPC : BaseCharacter
{

	[Prefab, Category( "Stats" )]
	public override float HitPoints { get; set; } = 6f;
	[Prefab, Category( "Stats" )]
	public virtual float AttackPower { get; set; } = 0.5f;
	[Prefab, Category( "Stats" )]
	public virtual float AttackSpeed { get; set; } = 0.5f;
	[Prefab, Category( "Stats" )]
	public virtual float AttackRange { get; set; } = 60f;
	[Prefab, Category( "Stats" )]
	public virtual float DetectRange { get; set; } = 300f;
	[Prefab, Category( "Stats" )] 
	public virtual float WalkSpeed { get; set; } = 120f;
	[Prefab, Category( "Stats" ), Description( "How diligent they are towards enacting their BaseBehaviour tasks. 0 = Never, 1 = Always")]
	public virtual float BaseDiligency { get; set; } = 0.5f;

	[Prefab, Category( "Stats" ), Description( "How many seconds it makes diligency check which determines if they will do their BaseBehaviour tasks" )]
	public virtual float DiligencyTimer { get; set; } = 3f;

	[Prefab, Category( "Character" )]
	public override FactionType Faction { get; set; } = FactionType.None;

	[Prefab, Category( "Character" )]
	public virtual Behaviour BaseBehaviour { get; set; } = Behaviour.None;

	[Prefab, Category( "Character" )]
	public virtual SubBehaviour BaseSubBehaviour { get; set; } = SubBehaviour.None;

	[Prefab, Category( "Character" )]
	public override float CollisionWidth { get; set; } = 20f;
	[Prefab, Category( "Character" )]
	public override float CollisionHeight { get; set; } = 40f;

	// Array of random strings that will popup when your goblin dies.
	private readonly string[] deathStrings = new string[]
	{
		"%attacker destroyed %target.",
		"%target died to %attacker.",
		"%target got chopped up by %attacker.",
		"%attacker killed %target.",
		"%target was killed by %attacker."
	};

	public BaseNPC() {}

	public override void Spawn()
	{
		base.Spawn();

		Tags.Add( "NPC" );

		CurrentBehaviour = BaseBehaviour;
		CurrentSubBehaviour = BaseSubBehaviour;

		Transmit = TransmitType.Pvs;
	}

	public override void Kill()
	{
		if ( CurrentTarget != null )
			CurrentTarget.TotalAttackers--;

		// Log Gobblin DN deaths...
		if ( Game.IsServer 
			&& Faction == FactionType.Goblins )
		{
			var input = deathStrings[Game.Random.Int( deathStrings.Length - 1 )]
				.Replace( "%attacker", LastAttackedBy?.DisplayName ?? "Unknown Entity" )
				.Replace( "%target", DisplayName );

			EventLogger.Send( To.Everyone, $"<red>{input}", 3 );
		}

		base.Kill();
	}

	public static BaseNPC FromPrefab( string prefabName )
	{
		if ( PrefabLibrary.TrySpawn<BaseNPC>( prefabName, out var npc ) )
		{
			return npc;
		}

		return null;
	}

	TimeUntil nextCheapThink = 0f;

	[Event.Tick.Server]
	public virtual void Think()
	{
		ComputeMotion();
		ComputeNavigation();

		if ( nextCheapThink )
		{
			nextCheapThink = 0.1f + Game.Random.Float( -0.03f, 0.03f );
			CheapThink();
		}
	}


	public virtual void CheapThink()
	{
		ComputeBehaviour();
		ComputeAnimations();

		if ( Position != Vector3.Zero && DefendingPosition == Vector3.Zero )
			DefendingPosition = Position;
	}


	[ConCmd.Admin("npc")]
	public static void SpawnTest( string type = "goblin", int amount = 1 )
	{
		var player = ConsoleSystem.Caller.Pawn as Lord;

		for( int i = 0; i < amount; i++ )
		{
			var guy = BaseNPC.FromPrefab( $"prefabs/npcs/{type}.prefab" );
			guy.Position = player.Position + Vector3.Random.WithZ( 0 ) * 100f;
		}
	}
}
