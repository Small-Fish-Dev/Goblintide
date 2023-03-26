using System.Linq;

namespace GameJam;

public enum Behaviour
{
	None,
	Raider, // Will take the behaviour of the raider, breaking stuff and attacking anyone of other factions
	Defender, // Will take the behaviour of the defender, moving closer to victims and attacking anyone of other factions
	Victim // Will take the behaviour of the victim, randomly walking unless there's raiders of opposing faction, then they run around panicking
}

public enum SubBehaviour
{
	None,
	Attacking,
	Stealing,
	Panicking,
	Guarding,
	Following
}
public partial class BaseNPC
{

	[Net] public Behaviour CurrentBehaviour { get; set; } = Behaviour.None;
	[Net] public SubBehaviour CurrentSubBehaviour { get; set; } = SubBehaviour.None;
	/* This is giving errors generating code :-//
	public Dictionary<Behaviour, Action<BaseNPC>> BehaviourTree = new Dictionary<Behaviour, Action<BaseNPC>>()
	{
		{ Behaviour.None, ( npc ) => {} },
		{ Behaviour.Raider, ( npc ) => { npc.RaiderBehaviour(); } },
		{ Behaviour.Defender, ( npc ) => { npc.DefenderBehaviour(); } },
		{ Behaviour.Victim, ( npc ) => { npc.VictimBehaviour(); } }
	};
	*/
	BaseCharacter currentTarget = null;
	public BaseCharacter CurrentTarget
	{
		get { return currentTarget; }
		set
		{
			if ( currentTarget.IsValid() )
				currentTarget.TotalAttackers--;
			currentTarget = value;
			if ( currentTarget.IsValid() )
				currentTarget.TotalAttackers++;
		}
	}
	public Vector3 CurrentTargetBestPosition { get; set; } = Vector3.Zero;
	public Vector3 LastKnownTargetPosition { get; set; } = Vector3.Zero;
	public Vector3 DefendingPosition { get; set; } = Vector3.Zero;
	public float DefendingPositionRange { get; set; } = 500f;

	public virtual void ComputeBehaviour()
	{
		//BehaviourTree[CurrentBehaviour].Invoke( this );

		if ( CurrentBehaviour == Behaviour.Defender )
			DefenderBehaviour();
		else if ( CurrentBehaviour == Behaviour.Raider )
			RaiderBehaviour();
		else if ( CurrentBehaviour == Behaviour.Victim )
			VictimBehaviour();
		else
			NoneBehaviour();
	}

	public virtual BaseCharacter FindBestTarget( float radius = 300f, bool closestFirst = true )
	{
		var validEntities = Entity.All
			.OfType<BaseCharacter>()
			.Where( x => x.Faction != FactionType.None && x.Faction != Faction )
			.Where( x => x.TotalAttackers < 3 );

		var radiusSquared = (float)Math.Pow( radius, 2 );

		if ( closestFirst )
		{
			validEntities.OrderBy( x => x.Position.DistanceSquared( Position ) );

			if ( validEntities.FirstOrDefault() != null )
				if ( validEntities.FirstOrDefault().Position.DistanceSquared( Position ) <= radiusSquared )
					return validEntities.FirstOrDefault();
		}
		else
			return validEntities.Where( x => x.Position.DistanceSquared( Position ) <= radiusSquared ).FirstOrDefault();

		return null;
	}
	public virtual Vector3 FindBestTargetPosition( BaseCharacter target, float distance = 0f )
	{
		if ( CurrentTarget == null ) return Vector3.Zero;

		var directionFromTarget = (Position - target.Position).WithZ( 0 ).Normal;
		var combinedBodyWidth = CollisionWidth / 2 + target.CollisionWidth / 2;
		var totalBestDistance = combinedBodyWidth + distance;

		return target.Position + directionFromTarget * totalBestDistance;
	}

	public bool FastRelativeInRangeCheck( BaseCharacter target, float distanceToCheck )
	{
		var combinedBodyWidth = CollisionWidth / 2 + target.CollisionWidth / 2;
		var combinedDistanceSquared = (float)Math.Pow(combinedBodyWidth + distanceToCheck, 2);

		return target.Position.DistanceSquared( Position ) <= combinedDistanceSquared;
	}

	public void RecalculateTargetNav()
	{
		LastKnownTargetPosition = CurrentTarget.Position;
		CurrentTargetBestPosition = FindBestTargetPosition( CurrentTarget, AttackRange / 2 );

		NavigateTo( CurrentTargetBestPosition );
	}

	TimeUntil nextTargetSearch { get; set; } = 0f;
	TimeUntil nextIdleMove { get; set; } = 0f;
	TimeUntil nextAttack { get; set; } = 0f;

	public void ComputeAttack( BaseCharacter target )
	{
		if ( nextAttack )
		{
			nextAttack = 1 / AttackSpeed + Game.Random.Float( -(1 / AttackSpeed / 10f ), 1 / AttackSpeed / 10f );
			target.Damage( AttackPower, this );

			SetAnimParameter( "b_attack", true );
		}
	}

	public void ComputeRevenge()
	{
		if ( !CurrentTarget.IsValid() && LastAttackedBy.IsValid() && LastAttackedBy != CurrentTarget )
		{
			CurrentTarget = LastAttackedBy;
		}
	}

	public void ComputeIdling()
	{
		CurrentSubBehaviour = SubBehaviour.None;

		if ( nextIdleMove )
		{
			nextIdleMove = Game.Random.Float( 2, 4 );

			var randomPositionAround = DefendingPosition + Vector3.Random.WithZ( 0 ) * DefendingPositionRange;
			NavigateTo( randomPositionAround );
		}
	}

	public virtual void NoneBehaviour()
	{
		ComputeIdling();
	}

	public virtual void AttackingSubBehaviour()
	{
		CurrentSubBehaviour = SubBehaviour.Attacking;

		Rotation = Rotation.LookAt( CurrentTarget.Position - Position );

		// Maybe make these checks once every 0.5 second if they prove laggy
		if ( CurrentTarget.Position.DistanceSquared( LastKnownTargetPosition ) >= (float)Math.Pow( AttackRange, 2 ) ) // If the target moved
			RecalculateTargetNav();

		if ( FastRelativeInRangeCheck( CurrentTarget, AttackRange ) )
			ComputeAttack( CurrentTarget );
		else
			RecalculateTargetNav();

		if ( !FastRelativeInRangeCheck( CurrentTarget, DetectRange ) )
			CurrentTarget = null;
	}

	public virtual void ComputeLookForTargets()
	{
		if ( nextTargetSearch )
		{
			nextTargetSearch = 1f;

			CurrentTarget = FindBestTarget( DetectRange, false ); // Any is fine, saves some computing
			if ( CurrentTarget.IsValid() )
				RecalculateTargetNav();
		}
	}

	public virtual void RaiderBehaviour()
	{
		if ( !CurrentTarget.IsValid() )
		{
			if ( CurrentSubBehaviour == SubBehaviour.Attacking || CurrentSubBehaviour == SubBehaviour.Following )
				CurrentSubBehaviour = BaseSubBehaviour;

			ComputeRevenge();
			ComputeLookForTargets();
		}
		else
		{
			AttackingSubBehaviour();
		}
	}

	public virtual void DefenderBehaviour()
	{
		if ( !CurrentTarget.IsValid() )
		{
			if ( CurrentSubBehaviour == SubBehaviour.Attacking || CurrentSubBehaviour == SubBehaviour.Following )
				CurrentSubBehaviour = BaseSubBehaviour;

			if ( CurrentSubBehaviour == SubBehaviour.Guarding || CurrentSubBehaviour == SubBehaviour.None )
			{
				ComputeIdling();
			}

			ComputeRevenge();
			ComputeLookForTargets();
		}
		else
		{
			AttackingSubBehaviour();
		}
	}
	public virtual void VictimBehaviour()
	{
		ComputeIdling();
	}
}
