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
	public virtual FactionType Faction { get; set; }
	public int TotalAttackers { get; set; } = 0;
	public BaseCharacter LastAttackedBy { get; set; } = null;
	public TimeSince LastAttacked { get; set; } = 0f;

	public BaseEntity() {}

	public virtual float GetWidth()
	{
		var bounds = PhysicsBody.GetBounds();
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
