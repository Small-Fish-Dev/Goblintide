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
	public BaseCharacter CurrentTarget { get; set; } = null;
	public Vector3 CurrentTargetBestPosition { get; set; } = Vector3.Zero;
	public Vector3 LastKnownTargetPosition {  get; set; } = Vector3.Zero;

	public virtual void ComputeBehaviour()
	{
		//BehaviourTree[CurrentBehaviour].Invoke( this );

		if ( CurrentBehaviour == Behaviour.Defender )
			DefenderBehaviour();
		else if ( CurrentBehaviour == Behaviour.Raider )
			RaiderBehaviour();
		else if ( CurrentBehaviour == Behaviour.Victim )
			VictimBehaviour();
	}

	TimeUntil nextTargetSearch { get; set; } = 0f;

	public virtual BaseCharacter FindBestTarget( float radius = 300f, bool closestFirst = true )
	{
		var validEntities = Entity.All
			.OfType<BaseCharacter>()
			.Where( x => x.Faction != FactionType.None && x.Faction != Faction )
			.Where( x => x.AttackedBy < 3 );

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

	public void RecalculateTargetNav()
	{
		LastKnownTargetPosition = CurrentTarget.Position;
		CurrentTargetBestPosition = FindBestTargetPosition( CurrentTarget, AttackRange / 2 );

		NavigateTo( CurrentTargetBestPosition );
	}

	public virtual void RaiderBehaviour()
	{
		if ( CurrentTarget == null )
		{
			if ( nextTargetSearch )
			{
				nextTargetSearch = 1f;

				CurrentTarget = FindBestTarget( DetectRange, false ); // Any is fine, saves some computing
				if ( CurrentTarget != null )
				{
					RecalculateTargetNav();
					CurrentTarget.AttackedBy++;
				}
			}
		}
		else
		{
			if ( CurrentTarget.Position.DistanceSquared( LastKnownTargetPosition ) >= (float)Math.Pow( AttackRange, 2 ) )
			{
				RecalculateTargetNav();
			}
		}
	}
	public virtual void DefenderBehaviour()
	{
		Log.Info( "I'm defending!" );
	}
	public virtual void VictimBehaviour()
	{
		Log.Info( "I'm victm!" );
	}
}
