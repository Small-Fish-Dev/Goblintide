using Sandbox.Component;

namespace GameJam;

public partial class Lord
{
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
						if ( pointingAt.Components.TryGet<Glow>( out Glow oldGlow ) )
							oldGlow.Enabled = false;

					if ( value.IsValid() )
					{
						var newGlow = value.Components.GetOrCreate<Glow>();
						newGlow.Color = Color.Red;
						newGlow.Enabled = true;
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
		if ( Pointing )
		{
			PointingAt = FindBestPointedAt();

			if ( Input.Pressed( InputButton.PrimaryAttack ) )
			{
				if ( PointingAt is BaseNPC npc )
				{
					if ( npc.Faction == Faction )
					{
						if ( CurrentlyCommanding.Contains( npc ) )
							RemoveFromCommanding( npc );
						else
							AddToCommanding( npc );
					}
				}

				if ( PointingAt.IsValid() )
				{
					var nearestAlly = FindClosestAlly();

					if ( nearestAlly.IsValid() )
					{
						nearestAlly.CurrentTarget = PointingAt;
						nearestAlly.IsFollowingOrder = true;
						nearestAlly.RecalculateTargetNav();
					}
				}
			}
		}
	}

	public BaseNPC FindClosestAlly()
	{
		BaseNPC closestAlly = null;

		var closeAllies = Entity.All
			.OfType<BaseNPC>()
			.Where( x => x.Faction == Faction )
			.Where( x => x.Position.DistanceSquared( PointingPosition ) <= Math.Pow( 1500, 2 ) );

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
		var preciseTrace = Trace.Ray( Camera.Position, Camera.Position + Camera.Rotation.Forward * 500f )
				.Size( 15f )
				.Ignore( this )
				.Run();

		if ( preciseTrace.Entity is BaseEntity preciseEntity )
		{
			PointingPosition = preciseEntity.Position;
			return preciseEntity;
		}

		var outerTrace = Trace.Ray( Camera.Position, Camera.Position + Camera.Rotation.Forward * 500f )
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
