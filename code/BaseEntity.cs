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

	public virtual float HitPoints { get; set; } = 6f;
	public virtual FactionType Faction { get; set; } = FactionType.None;
	public int TotalAttackers { get; set; } = 0;
	public BaseCharacter LastAttackedBy { get; set; } = null;
	public TimeSince LastAttacked { get; set; } = 0f;
	public virtual bool BlockNav { get; set; } = true;

	public BaseEntity() {}

	public override void OnNewModel( Model model )
	{
		base.OnNewModel( model );

		if ( !BlockNav ) return;

		var navBlocker = new NavBlockerEntity();

		navBlocker.PhysicsClear();
		navBlocker.SetModel( model.ResourcePath );
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
