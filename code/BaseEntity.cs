namespace GameJam;

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
	public virtual float MaxHitPoints { get; set; } = 6f;
	[Net] public float HitPoints { get; set; } = 6f;
	[Prefab, Category( "Stats" )]
	public virtual FactionType DefaultFaction { get; set; } = FactionType.None;
	[Net] public FactionType Faction { get; set; } = FactionType.None;
	public int TotalAttackers { get; set; } = 0;
	public BaseCharacter LastAttackedBy { get; set; } = null;
	public TimeSince LastAttacked { get; set; } = 0f;
	public virtual bool BlockNav { get; set; } = true;

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

		var navBlocker = new NavBlockerEntity();
		navBlocker.PhysicsClear();
		navBlocker.Model = model;
		navBlocker.SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
		navBlocker.Position = Position;
		navBlocker.Rotation = Rotation;
		navBlocker.PhysicsEnabled = false;
		navBlocker.EnableDrawing = false;
		navBlocker.Enable();
		navBlocker.SetParent( this );
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
		var bounds = PhysicsBody.GetBounds();
		
		return bounds.Maxs.z - bounds.Mins.z;
	}



	public void Glow( bool on )
	{
		var currentLord = Game.LocalPawn as Lord;

		if ( on )
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
