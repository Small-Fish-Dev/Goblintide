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
	BaseEntity currentTarget = null;
	public BaseEntity CurrentTarget
	{
		get { return currentTarget; }
		set
		{
			IsFollowingOrder = false;
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
	public bool IsDiligent { get; set; } = true;
	public bool IsFollowingOrder { get; set; } = false;

	public virtual void ComputeBehaviour()
	{
		//BehaviourTree[CurrentBehaviour].Invoke( this );
		DiligencyCheck();

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
			return validEntities.FirstOrDefault( x => x.Position.DistanceSquared( Position ) <= radiusSquared );

		return null;
	}

	public virtual BaseProp FindBestProp( float radius = 300f, bool closestFirst = true )
	{
		var validEntities = Entity.All
			.OfType<BaseProp>()
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
			return validEntities.FirstOrDefault( x => x.Position.DistanceSquared( Position ) <= radiusSquared );

		return null;
	}

	public virtual Vector3 FindBestTargetPosition( BaseEntity target, float distance = 0f )
	{
		if ( CurrentTarget == null ) return Vector3.Zero;

		var directionFromTarget = (Position - target.Position).WithZ( 0 ).Normal;
		var combinedBodyWidth = GetWidth() / 2 + target.GetWidth() / 2;
		var totalBestDistance = combinedBodyWidth + distance;

		return target.Position + directionFromTarget * totalBestDistance;
	}

	public bool FastRelativeInRangeCheck( BaseEntity target, float distanceToCheck )
	{
		var combinedBodyWidth = GetWidth() / 2 + target.GetWidth() / 2;
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
	TimeUntil nextMove { get; set; } = 0f;
	TimeUntil nextAttack { get; set; } = 0f;
	TimeUntil nextDiligencyCheck { get; set; } = 0f;

	public void ComputeAttack( BaseEntity target )
	{
		if ( nextAttack )
		{
			nextAttack = 1 / AttackSpeed + Game.Random.Float( -(1 / AttackSpeed / 10f ), 1 / AttackSpeed / 10f );
			target.Damage( AttackPower, this );

			SetAnimParameter( "b_attack", true );
			PlayAttackSound();
		}
	}

	public void ComputeRevenge()
	{
		if ( LastAttackedBy.IsValid() && LastAttackedBy != CurrentTarget )
		{
			CurrentTarget = LastAttackedBy;
		}
	}

	public void ComputeIdling()
	{
		CurrentSubBehaviour = SubBehaviour.None;

		if ( nextMove )
		{
			nextMove = Game.Random.Float( 2, 4 );

			var randomPositionAround = DefendingPosition + Vector3.Random.WithZ( 0 ) * DefendingPositionRange;
			NavigateTo( randomPositionAround );

			if ( Game.Random.Float() <= 0.1f )
				PlayLaughSound();
			else
				PlayIdleSound();
		}
	}

	public virtual void NoneBehaviour()
	{
		ComputeIdling();
	}

	public virtual void AttackingSubBehaviour()
	{
		CurrentSubBehaviour = SubBehaviour.Attacking;

		if ( CurrentTarget.Position.DistanceSquared( LastKnownTargetPosition ) >= (float)Math.Pow( AttackRange, 2 ) ) // If the target moved
			RecalculateTargetNav();

		if ( FastRelativeInRangeCheck( CurrentTarget, AttackRange ) )
			ComputeAttack( CurrentTarget );
		else
		{
			if ( !IsFollowingPath )
				RecalculateTargetNav();
		}

		if ( !FastRelativeInRangeCheck( CurrentTarget, DetectRange ) && !IsFollowingOrder )
			CurrentTarget = null;
	}

	public virtual void PanickingSubBehaviour()
	{
		CurrentSubBehaviour = SubBehaviour.Panicking;
		
		if ( FastRelativeInRangeCheck( CurrentTarget, DetectRange ) )
		{
			if ( nextMove )
			{
				nextMove = Game.Random.Float( 0.5f, 1f );

				var randomPositionAround = DefendingPosition + Vector3.Random.WithZ( 0 ) * DefendingPositionRange;
				NavigateTo( randomPositionAround );
			}
		}
		else
			CurrentTarget = null;
	}

	public virtual void DiligencyCheck()
	{
		if ( nextDiligencyCheck )
		{
			nextDiligencyCheck = DiligencyTimer;

			IsDiligent = Game.Random.Float( 0f, 1f ) <= BaseDiligency;
		}
	}

	public virtual void ComputeLookForTargets()
	{
		if ( CurrentTarget.IsValid() ) return;

		CurrentTarget = FindBestTarget( DetectRange, false ); // Any is fine, saves some computing

		if ( CurrentTarget.IsValid() )
			RecalculateTargetNav();
	}

	public virtual void ComputeLookForProps()
	{
		if ( CurrentTarget.IsValid() ) return;

		CurrentTarget = FindBestProp( DetectRange, false ); // Any is fine, saves some computing

		if ( CurrentTarget.IsValid() )
			RecalculateTargetNav();
	}

	public virtual void RaiderBehaviour()
	{
		if ( !CurrentTarget.IsValid() )
		{
			if ( CurrentSubBehaviour is SubBehaviour.Attacking or SubBehaviour.Following )
				CurrentSubBehaviour = BaseSubBehaviour;

			if ( CurrentSubBehaviour is SubBehaviour.Guarding or SubBehaviour.None )
				ComputeIdling();

			if ( nextTargetSearch )
			{
				nextTargetSearch = 1f;

				if ( IsDiligent )
				{
					ComputeLookForTargets();
					ComputeLookForProps();
				}
			}
		}
		else
		{
			AttackingSubBehaviour();
		}

		ComputeRevenge();
	}

	public virtual void DefenderBehaviour()
	{
		if ( !CurrentTarget.IsValid() )
		{
			if ( CurrentSubBehaviour is SubBehaviour.Attacking or SubBehaviour.Following )
				CurrentSubBehaviour = BaseSubBehaviour;

			if ( CurrentSubBehaviour is SubBehaviour.Guarding or SubBehaviour.None )
				ComputeIdling();

			if ( nextTargetSearch )
			{
				nextTargetSearch = 1f;

				if ( IsDiligent )
				{
					ComputeRevenge();
					ComputeLookForTargets();
				}
			}
		}
		else
		{
			AttackingSubBehaviour();
		}

		ComputeRevenge();
	}
	public virtual void VictimBehaviour()
	{
		if ( !CurrentTarget.IsValid() )
		{
			if ( CurrentSubBehaviour == SubBehaviour.Panicking )
				CurrentSubBehaviour = BaseSubBehaviour;

			if ( CurrentSubBehaviour == SubBehaviour.None )
				ComputeIdling();

			if ( nextTargetSearch )
			{
				nextTargetSearch = 1f; 
				
				if ( IsDiligent )
					CurrentTarget = FindBestTarget( DetectRange, false );
			}
		}
		else
		{
			PanickingSubBehaviour();
		}
	}
}
