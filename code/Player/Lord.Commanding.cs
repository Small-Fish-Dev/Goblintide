using Sandbox.Component;

namespace GameJam;

public partial class Lord
{
	public float PointingDistance { get; set; } = 500f;
	internal BaseEntity pointingAt { get; set; } = null;
	public BaseEntity PointingAt
	{
		get => pointingAt;
		set
		{
			if ( Game.IsClient )
			{
				if ( value != pointingAt )
				{
					if ( pointingAt.IsValid() )
						pointingAt.Glow( false );

					if ( value.IsValid() )
					{
						value.Glow( true );
					}
				}
			}

			pointingAt = value;
		}
	}
	[Net] public Vector3 PointingPosition { get; set; } = 0f;
	public List<BaseNPC> CurrentlyCommanding { get; set; } = new();
	public void SimulateCommanding()
	{
		if ( GetAnimParameterInt( "state" ) == 4 )
			SetAnimParameter( "state", 0 );

		if ( Pointing )
		{
			PointingAt = FindBestPointedAt();

			if ( Input.Pressed( InputButton.PrimaryAttack ) )
			{
				if ( PointingAt.IsValid() )
				{
					PlaySound( "sounds/lord/lord_command.sound" );
					if ( PointingAt.Faction == Faction )
					{
						if ( PointingAt is BaseNPC npc )
						{
							if ( CurrentlyCommanding.Contains( npc ) )
								RemoveFromCommanding( npc );
							else
								AddToCommanding( npc );
						}
					}
					else
					{
						var nearestAlly = FindClosestAlly();

						if ( nearestAlly.IsValid() )
						{
							nearestAlly.CurrentTarget = PointingAt;
							nearestAlly.IsFollowingOrder = true;
							nearestAlly.RecalculateTargetNav();
						}
					}

					SetAnimParameter( "state", 4 );
				}
			}
		}
		else
		{
			if ( PointingAt.IsValid() )
			{
				if ( PointingAt.Components.TryGet<Glow>( out Glow oldGlow ) )
					oldGlow.Enabled = false;
				PointingAt = null;
			}
			PointingAt = null;
		}
	}

	public BaseNPC FindClosestAlly()
	{
		BaseNPC closestAlly = null;

		var closeAllies = Entity.All
			.OfType<BaseNPC>()
			.Where( x => x.Faction == Faction )
			.Where( x => !x.Stealing.IsValid() )
			.Where( x => x.Position.DistanceSquared( PointingPosition ) <= Math.Pow( 1500, 2 ) );

		if ( PointingAt is BaseItem item )
		{
			if ( item.Type == ItemType.Armor )
				closeAllies.Where( x => !x.Armor.IsValid() );

			if ( item.Type == ItemType.Weapon )
				closeAllies.Where( x => !x.Weapon.IsValid() );
		}

		var freeAllies = closeAllies
			.Where( x => x.CurrentSubBehaviour != SubBehaviour.Attacking );

		if ( freeAllies.Count() > 0 ) // Find allies that aren't busy
		{
			closestAlly = freeAllies
				.OrderBy( x => x.Position.DistanceSquared( PointingPosition ) )
				.FirstOrDefault();
		}
		else
		{
			closestAlly = closeAllies
				.OrderBy( x => x.Position.DistanceSquared( PointingPosition ) )
				.FirstOrDefault();
		}

		return closestAlly;
	}

	public void AddToCommanding( BaseNPC npc )
	{
		CurrentlyCommanding.Add( npc );
	}

	public void RemoveFromCommanding( BaseNPC npc )
	{
		CurrentlyCommanding.Remove( npc );
	}

	internal BaseEntity FindBestPointedAt()
	{
		var preciseTrace = Trace.Ray( Camera.Position, Camera.Position + Camera.Rotation.Forward * PointingDistance )
			.Ignore( this )
			.Run();

		if ( preciseTrace.Entity is BaseEntity preciseEntity )
		{
			PointingPosition = preciseEntity.Position;
			return preciseEntity;
		}

		var innerTrace = Trace.Ray( Camera.Position, Camera.Position + Camera.Rotation.Forward * PointingDistance )
			.Size( 10f )
			.Ignore( this )
			.Run();

		if ( innerTrace.Entity is BaseEntity innerEntity )
		{
			PointingPosition = innerEntity.Position;
			return innerEntity;
		}

		var outerTrace = Trace.Ray( Camera.Position, Camera.Position + Camera.Rotation.Forward * PointingDistance )
			.Size( 30f )
			.Ignore( this )
			.Run();

		if ( outerTrace.Entity is BaseEntity outerEntity )
		{
			PointingPosition = outerEntity.Position;
			return outerEntity;
		}

		var closestEntityToPrecise = Entity.All
			.OfType<BaseEntity>()
			.Where( x => x.Position.DistanceSquared( preciseTrace.HitPosition ) <= 900f )
			.OrderBy( x => x.Position.DistanceSquared( preciseTrace.HitPosition ) )
			.FirstOrDefault();

		if ( closestEntityToPrecise.IsValid() )
		{
			PointingPosition = closestEntityToPrecise.Position;
			return closestEntityToPrecise;
		}

		PointingPosition = preciseTrace.HitPosition;

		return null;
	}
}
