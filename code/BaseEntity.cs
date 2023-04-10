namespace GoblinGame;

public enum FactionType
{
	None,
	Goblins,
	Humans,
	Nature
}

public partial class BaseEntity : AnimatedEntity
{

	[Prefab, Category( "Stats" )]
	[Net] public float MaxHitPoints { get; set; } = 6f;
	[Net] public float HitPoints { get; set; } = 6f;
	[Prefab, Category( "Stats" )]
	public virtual FactionType DefaultFaction { get; set; } = FactionType.None;
	[Net] public FactionType Faction { get; set; } = FactionType.None;
	public int TotalAttackers { get; set; } = 0;
	public BaseCharacter LastAttackedBy { get; set; } = null;
	public TimeSince LastAttacked { get; set; } = 0f;
	public virtual bool BlockNav { get; set; } = true;
	public NavBlockerEntity NavBlocker { get; set; } = null;
	public virtual string DamageSound { get; set; } = "sounds/physics/physics.wood.impact.soft.sound";

	public override void Spawn()
	{
		base.Spawn();

		Faction = DefaultFaction;
		HitPoints = MaxHitPoints;
	}

	public override void OnNewModel( Model model )
	{
		base.OnNewModel( model );

		if ( !BlockNav || model == null ) 
			return;

		NavBlocker = new NavBlockerEntity();
		NavBlocker.PhysicsClear();
		NavBlocker.Model = model;
		NavBlocker.SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
		NavBlocker.Position = Position;
		NavBlocker.Rotation = Rotation;
		NavBlocker.PhysicsEnabled = false;
		NavBlocker.EnableDrawing = false;
		NavBlocker.Enable();
		NavBlocker.SetParent( this );
	}

	public virtual float GetWidth()
	{
		if ( !PhysicsBody.IsValid() ) return 0;

		var bounds = CollisionBounds;
		var maxMins = Math.Min( bounds.Mins.x, bounds.Mins.y );
		var maxMaxs = Math.Max( bounds.Maxs.x, bounds.Maxs.y );

		return maxMaxs - maxMins;
	}

	public virtual float GetHeight()
	{
		if ( !PhysicsBody.IsValid () ) return 0;

		var bounds = PhysicsBody.GetBounds();
		
		return bounds.Maxs.z - bounds.Mins.z;
	}



	public void Glow( bool on )
	{
		var currentLord = Game.LocalPawn as Lord;

		if ( !on )
		{
			if ( Components.TryGet<Glow>( out Glow oldGlow ) )
				oldGlow.Enabled = false;

			foreach ( var child in Children )
			{
				if ( child.Components.TryGet<Glow>( out Glow childOldGlow ) )
					childOldGlow.Enabled = false;
			}
		}
		else
		{
			var newGlow = Components.GetOrCreate<Glow>();
			newGlow.Enabled = true;
			newGlow.Color = currentLord?.Faction == Faction ? Color.Green : Color.Red;

			foreach ( var child in Children )
			{
				var newChildGlow = child.Components.GetOrCreate<Glow>();
				newChildGlow.Enabled = true;
				newChildGlow.Color = currentLord?.Faction == Faction ? Color.Green : Color.Red;
			}
		}
	}

	public virtual void Damage( float amount, BaseCharacter attacker )
	{
		HitPoints = Math.Max( HitPoints - amount, 0 );
		LastAttacked = 0f;
		LastAttackedBy = attacker;

		Sound.FromWorld( DamageSound, Position ).SetVolume( 3f );

		if ( HitPoints <= 0 )
		{
			Kill();
		}
	}

	public virtual void Kill()
	{
		Delete();
	}
}
