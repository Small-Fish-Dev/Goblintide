﻿using GoblinGame.Props.Collectable;
using Sandbox.UI;
using System.Linq;
using System.Reflection.Metadata.Ecma335;

namespace GoblinGame;

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
	Following,
	Equipping
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
	public bool IsGoingToForest { get; set; } = false;

	public virtual void ComputeBehaviour()
	{
		if ( Goblintide.CurrentTown == null ) return;
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
			.Where( x => !x.Disabled )
			.Where( x => !x.Dead )
			.Where( x => x.Faction != FactionType.None && x.Faction != Faction )
			.Where( x => x.TotalAttackers < 3 );

		var radiusSquared = radius * radius;

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

	public virtual BaseItem FindBestItem( float radius = 300f, bool closestFirst = true )
	{
		var validEntities = Entity.All
			.OfType<BaseItem>()
			.Where( x => !x.Equipped);

		var radiusSquared = radius * radius;

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

	public virtual BaseCollectable FindBestLoot( float radius = 300f, bool closestFirst = true )
	{
		var validEntities = Entity.All
			.OfType<BaseCollectable>()
			.Where( x => !x.Locked)
			.Where( x => !x.StolenBy.IsValid());

		var radiusSquared = radius * radius;

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

		var radiusSquared = radius * radius;

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

		return target.Position.WithZ(0).DistanceSquared( Position.WithZ(0) ) <= combinedDistanceSquared;
	}

	public async void RecalculateTargetNav()
	{
		if ( CurrentGrid == null ) return;
		LastKnownTargetPosition = CurrentTarget.Position;
		CurrentTargetBestPosition = FindBestTargetPosition( CurrentTarget, AttackRange / 2 );

		await NavigateTo( CurrentGrid.GetNearestCell( CurrentTargetBestPosition, true, true ) );
	}

	TimeUntil nextTargetSearch { get; set; } = 0f;
	TimeUntil nextMove { get; set; } = 0f;
	TimeUntil nextAttack { get; set; } = 0f;
	TimeUntil nextDiligencyCheck { get; set; } = 0f;

	public void ComputeAttack( BaseEntity target )
	{
		if ( nextAttack )
		{
			if ( Weapon.IsValid() )
			{
				if ( Weapon.IncreasedRange > 50f )
				{
					var particle = Particles.Create( "particles/magic_shock.vpcf" );
					particle.SetPosition( 0, Weapon.Position );
					particle.SetPosition( 1, target.Position + target.GetHeight() / 2f );
				}
			}

			nextAttack = 1 / AttackSpeed + Game.Random.Float( -(1 / AttackSpeed / 10f ), 1 / AttackSpeed / 10f );
			target.Damage( AttackPower, this );

			SetAnimParameter( "Attack", true );

			PlayAttackSound();

			experience += target is BaseNPC ? 0.2f : 0.05f;
			if ( experience >= 1f + Level / 5f )
			{
				experience = 0f;
				IncreaseLevel();
			}
		}
	}

	public void ComputeRevenge()
	{
		if ( LastAttackedBy.IsValid() && LastAttackedBy != CurrentTarget && !LastAttackedBy.Dead )
		{
			CurrentTarget = LastAttackedBy;
		}
	}

	public void ComputeIdling()
	{
		CurrentSubBehaviour = SubBehaviour.None;

		if ( nextMove )
		{
			if ( CurrentGrid == null ) return;
			nextMove = Game.Random.Float( 2, 4 );

			var randomPositionAround = DefendingPosition + Vector3.Random.WithZ( 0 ) * DefendingPositionRange;
			GameTask.RunInThreadAsync( () =>
			{
				NavigateTo( CurrentGrid.GetCell( randomPositionAround ) );
			} );

			if ( Game.Random.Float() <= 0.1f )
				PlayLaughSound();
			else
				PlayIdleSound();
		}
	}

	public virtual void NoneBehaviour()
	{
		ComputeIdling();
		IsGoingToForest = false;
	}

	public virtual void Steal( BaseCollectable collectable )
	{
		//IsFollowingPath = false;
		Stealing = collectable;
		Stealing.Locked = true;
		Stealing.Position = Position + Vector3.Up * GetHeight();
		Stealing.SetParent( this );
	}

	public float DistanceToForest()
	{
		var relativeTownPosition = (Position - Goblintide.CurrentTown.Position);
		return Goblintide.CurrentTown.ForestRadius - relativeTownPosition.Length;
	}

	public virtual void NavigateToForest()
	{
		var relativeTownPosition = (Position - Goblintide.CurrentTown.Position).Normal;
		var bestEscapePosition = Goblintide.CurrentTown.Position + relativeTownPosition * Goblintide.CurrentTown.ForestRadius;

		IsGoingToForest = true;
		GameTask.RunInThreadAsync( () =>
		{
			NavigateTo( CurrentGrid.GetCell( bestEscapePosition ) );
		} );
	}

	public virtual void EquippingSubBehaviour()
	{
		CurrentSubBehaviour = SubBehaviour.Equipping;
		var target = CurrentTarget as BaseItem;

		if ( !target.IsValid() || target.Equipped )
		{
			CurrentTarget = null;
			return;
		}

		if ( Armor.IsValid() && target.Type == ItemType.Armor )
		{
			CurrentTarget = null;
			return;
		}

		if ( Weapon.IsValid() && target.Type == ItemType.Weapon )
		{
			CurrentTarget = null;
			return;
		}

		if ( FastRelativeInRangeCheck( CurrentTarget, AttackRange ) )
			Equip( target );
		else if ( !IsFollowingPath )
			RecalculateTargetNav();

		if ( !FastRelativeInRangeCheck( CurrentTarget, DetectRange ) && !IsFollowingOrder )
			CurrentTarget = null;
	}

	public virtual void StealingSubBehaviour()
	{
		CurrentSubBehaviour = SubBehaviour.Stealing;

		if ( !Stealing.IsValid() )
		{
			var collectable = CurrentTarget as BaseCollectable;

			if ( collectable.IsValid() )
				if ( collectable.StolenBy.IsValid() )
				{
					CurrentTarget = null;
					return;
				}

			if ( FastRelativeInRangeCheck( CurrentTarget, AttackRange ) )
			{
				if ( collectable.IsValid() )
					Steal( collectable );
			}
			else
			{
				if ( !IsFollowingPath )
					RecalculateTargetNav();
			}

			if ( !FastRelativeInRangeCheck( CurrentTarget, DetectRange ) && !IsFollowingOrder )
				CurrentTarget = null;
		}
		else
		{
			if ( !IsFollowingPath )
			{
				NavigateToForest();
				IsFollowingOrder = true;
			}

			if ( DistanceToForest() <= 100f )
			{
				Goblintide.AddResource( Stealing.Type, Stealing.Value );
				Stealing.Delete();
				IsGoingToForest = false;
			}
		}
		
	}

	public virtual void AttackingSubBehaviour()
	{
		if ( CurrentTarget is BaseNPC npc )
			if ( npc.Dead )
			{
				CurrentTarget = null;
				return;
			}	
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
				GameTask.RunInThreadAsync( () =>
				{
					NavigateTo( CurrentGrid.GetCell( randomPositionAround ) );
				} );
				PlayPanicSound();
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

		CurrentTarget = FindBestTarget( DetectRange, true );

		if ( CurrentTarget.IsValid() )
			RecalculateTargetNav();
	}

	public virtual void ComputeLookForProps()
	{
		if ( CurrentTarget.IsValid() ) return;

		CurrentTarget = FindBestProp( DetectRange, false );

		if ( CurrentTarget.IsValid() )
			RecalculateTargetNav();
	}

	public virtual void ComputeLookForLoot()
	{
		if ( CurrentTarget.IsValid() ) return;

		CurrentTarget = FindBestLoot( DetectRange, false );

		if ( CurrentTarget.IsValid() )
			RecalculateTargetNav();
	}

	public virtual void ComputeLookForItems()
	{
		if ( CurrentTarget.IsValid() ) return;

		CurrentTarget = FindBestItem( DetectRange, false );

		if ( CurrentTarget.IsValid() )
			RecalculateTargetNav();
	}

	internal TimeUntil newDefendingPosition { get; set; } = 0f;

	public virtual void FindNewPatrolPosition()
	{
		if ( newDefendingPosition )
		{
			var currentTown = Goblintide.CurrentTown;
			DefendingPosition = currentTown.Position + Vector3.Random.WithZ(0).Normal * Game.Random.Float( -currentTown.TownRadius, currentTown.TownRadius );
			IsFollowingOrder = true;
			newDefendingPosition = Game.Random.Float( 8f, 16f );
		}
	}

	public virtual void RaiderBehaviour()
	{
		if ( !CurrentTarget.IsValid() )
		{
			if ( CurrentSubBehaviour is not SubBehaviour.None )
			{
				CurrentSubBehaviour = BaseSubBehaviour;
				IsGoingToForest = false;
			}

			if ( CurrentSubBehaviour is SubBehaviour.Guarding or SubBehaviour.None )
			{
				FindNewPatrolPosition();
				ComputeIdling();
			}

			if ( nextTargetSearch )
			{
				nextTargetSearch = 1f;

				if ( IsDiligent )
				{
					ComputeLookForItems();

					if ( !CurrentTarget.IsValid() ) 
						ComputeLookForLoot();

					if ( !CurrentTarget.IsValid() )
					{
						var randomChoice = Game.Random.Int( 2 );
						if ( randomChoice == 0 )
							ComputeLookForTargets();
						else if ( randomChoice == 1 )
							ComputeLookForProps();
					}
				}
			}
		}
		else
		{
			if ( CurrentTarget is BaseCollectable )
				StealingSubBehaviour();
			else if ( CurrentTarget is BaseItem )
				EquippingSubBehaviour();
			else
				AttackingSubBehaviour();

			if ( CurrentTarget is BaseNPC npc )
				if ( npc.Dead )
					CurrentTarget = null;
		}

		ComputeRevenge();
	}

	public virtual void DefenderBehaviour()
	{
		if ( !CurrentTarget.IsValid() )
		{
			if ( CurrentSubBehaviour is SubBehaviour.Attacking or SubBehaviour.Following )
			{
				CurrentSubBehaviour = BaseSubBehaviour;
				IsGoingToForest = false;
			}

			if ( CurrentSubBehaviour is SubBehaviour.Guarding or SubBehaviour.None )
			{
				FindNewPatrolPosition();
				ComputeIdling();
			}

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

			if ( CurrentTarget is BaseNPC npc )
				if ( npc.Dead )
					CurrentTarget = null;
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
			{
				FindNewPatrolPosition();
				ComputeIdling();
			}

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

			if ( CurrentTarget is BaseNPC npc )
				if ( npc.Dead )
					CurrentTarget = null;
		}
	}
}
