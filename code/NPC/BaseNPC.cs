using GameJam.Props.Collectable;

namespace GameJam;

public enum VoiceType
{
	None,
	Goblin,
	Hobgoblin,
	Human
}

[Flags]
public enum Bodygroups
{
	None = 0,
	Head = 1,
	Chest = 2,
	Legs = 4,
	Hands = 8,
	Feet = 16,
}

[Prefab, Category( "NPC" )]
public partial class BaseNPC : BaseCharacter
{

	
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
	public virtual Behaviour BaseBehaviour { get; set; } = Behaviour.None;

	[Prefab, Category( "Character" )]
	public virtual VoiceType Voice { get; set; } = VoiceType.None;

	[Prefab, Category( "Character" )]
	public virtual SubBehaviour BaseSubBehaviour { get; set; } = SubBehaviour.None;

	[Prefab, Category( "Character" )]
	public override float CollisionWidth { get; set; } = 20f;
	[Prefab, Category( "Character" )]
	public override float CollisionHeight { get; set; } = 40f;
	[Prefab, Category( "Character" )]
	public Bodygroups BodygroupsDisabled { get; set; } = Bodygroups.None;
	public BaseCollectable Stealing { get; set; } = null;

	[Prefab, Category( "Character" ), ResourceType( "prefab" )]
	public string StartingWeapon { get; set; } = null;
	[Prefab, Category( "Character" ), ResourceType( "prefab" )]
	public string StartingArmor { get; set; } = null;

	[Net] public BaseItem Weapon { get; set; } = null;
	[Net] public BaseItem Armor { get; set; } = null;

	// Array of random strings that will popup when your goblin dies.
	private readonly string[] deathStrings = new string[]
	{
		"<lightblue>%attacker</> destroyed <lightgreen>%target.</>",
		"<lightgreen>%target</> died to <lightblue>%attacker.</>",
		"<lightgreen>%target</> got chopped up by <lightblue>%attacker.</>",
		"<lightblue>%attacker</> killed <lightgreen>%target.</>",
		"<lightgreen>%target</> was killed by <lightblue>%attacker.</>"
	};

	public override void Spawn()
	{
		base.Spawn();

		Faction = DefaultFaction;
		HitPoints = MaxHitPoints;
		Tags.Add( "NPC" );

		CurrentBehaviour = BaseBehaviour;
		CurrentSubBehaviour = BaseSubBehaviour;

		Transmit = TransmitType.Pvs;

		if ( BodygroupsDisabled.HasFlag( Bodygroups.Head ) ) SetBodyGroup( "Head", 1 );
		if ( BodygroupsDisabled.HasFlag( Bodygroups.Chest ) ) SetBodyGroup( "Chest", 1 );
		if ( BodygroupsDisabled.HasFlag( Bodygroups.Legs ) ) SetBodyGroup( "Legs", 1 );
		if ( BodygroupsDisabled.HasFlag( Bodygroups.Hands ) ) SetBodyGroup( "Hands", 1 );
		if ( BodygroupsDisabled.HasFlag( Bodygroups.Feet ) ) SetBodyGroup( "Feet", 1 );

		if ( StartingArmor != null )
		{
			var armor = BaseItem.FromPrefab( StartingArmor );
			if ( armor != null )
				Equip( armor );
		}

		if ( StartingWeapon != null )
		{
			var weapon = BaseItem.FromPrefab( StartingWeapon );
			if ( weapon != null )
				Equip( weapon );
		}
	}

	public void Equip( BaseItem item )
	{
		if ( !item.IsValid() ) return;
		item.EnableAllCollisions = false;
		item.SetParent( this, true );

		item.Equipped = true;
		HitPoints += item.IncreasedHealth;
		AttackPower += item.IncreasedAttack;
	}

	public void Drop( BaseItem item )
	{
		if ( !item.IsValid() ) return;
		item.SetParent( null );
		item.EnableAllCollisions = true;

		item.Equipped = false;
	}

	public override void Kill()
	{
		if ( CurrentTarget != null )
			CurrentTarget.TotalAttackers--;

		Drop( Armor );
		Drop( Weapon );

		// Log Gobblin DN deaths...
		if ( Game.IsServer 
			&& Faction == FactionType.Goblins )
		{
			var input = deathStrings[Game.Random.Int( deathStrings.Length - 1 )]
				.Replace( "%attacker", LastAttackedBy?.DisplayName ?? "Unknown Entity" )
				.Replace( "%target", DisplayName );

			EventLogger.Send( To.Everyone, $"{input}", 3 );
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

	public Dictionary<VoiceType, string> AttackSounds = new()
	{
		{ VoiceType.None, "" },
		{ VoiceType.Goblin, "sounds/golbins/goblin_attack.sound" },
	};

	public Dictionary<VoiceType, string> HurtSounds = new()
	{
		{ VoiceType.None, "" },
		{ VoiceType.Goblin, "sounds/golbins/goblin_hurt.sound" },
	};

	public Dictionary<VoiceType, string> IdleSounds = new()
	{
		{ VoiceType.None, "" },
		{ VoiceType.Goblin, "sounds/golbins/goblin_idle.sound" },
	};

	public Dictionary<VoiceType, string> LaughSounds = new()
	{
		{ VoiceType.None, "" },
		{ VoiceType.Goblin, "sounds/golbins/goblin_laugh.sound" },
	};

	public virtual void PlayAttackSound()
	{
		PlaySound( AttackSounds[Voice] );
	}

	public virtual void PlayHurtSound()
	{
		PlaySound( HurtSounds[Voice] );
	}

	public virtual void PlayIdleSound()
	{
		PlaySound( IdleSounds[Voice] );
	}

	public virtual void PlayLaughSound()
	{
		PlaySound( LaughSounds[Voice] );
	}

	public override void Damage( float amount, BaseCharacter attacker )
	{
		base.Damage( amount, attacker );
		PlayHurtSound();
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
