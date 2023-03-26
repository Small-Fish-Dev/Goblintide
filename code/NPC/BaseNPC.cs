namespace GameJam;

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

	public BaseNPC() {}

	public override void Spawn()
	{
		base.Spawn();

		Tags.Add( "NPC" );

		CurrentBehaviour = BaseBehaviour;
		CurrentSubBehaviour = BaseSubBehaviour;
	}

	public override void Kill()
	{
		if ( CurrentTarget != null )
			CurrentTarget.TotalAttackers--;

		base.Kill();
	}

	public static BaseNPC FromPrefab( string prefabName )
	{
		if ( PrefabLibrary.TrySpawn<BaseNPC>( prefabName, out var settler ) )
		{
			return settler;
		}

		return null;
	}


	[Event.Tick.Server]
	public virtual void Think()
	{
		ComputeNavigation();
		ComputeMotion();
		ComputeAnimations();
		ComputeBehaviour();

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
