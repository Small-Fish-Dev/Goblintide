using Sandbox.Internal;
using System.Collections.Immutable;

namespace GoblinGame;

public partial class BaseNPC
{
	internal ImmutableArray<GridAStar.Cell> currentPath { get; set; } = ImmutableArray<GridAStar.Cell>.Empty;
	public int CurrentPathLength => currentPath.Length;
	internal int currentPathIndex { get; set; } = -1; // -1 = Not set / Hasn't started
	internal GridAStar.Cell currentPathCell => IsFollowingPath ? currentPath[currentPathIndex] : null;
	internal GridAStar.Cell lastPathCell => currentPath.Length > 0 ? currentPath[^1] : null;
	internal GridAStar.Cell targetPathCell { get; set; } = null;
	internal GridAStar.Cell nextPathCell => IsFollowingPath ? currentPath[Math.Min( currentPathIndex + 1, currentPath.Length - 1 )] : null;
	public bool IsFollowingPath => currentPathIndex >= 0 && currentPath.Length > 0;
	public bool HasArrivedDestination { get; internal set; } = false;
	public virtual float PathRetraceFrequency { get; set; } = 0.1f; // How many seconds before it checks if the path is being followed or the target position changed
	internal TimeUntil lastRetraceCheck { get; set; } = 0f;
	public GridAStar.Grid CurrentGrid => GridAStar.Grid.Main;
	public GridAStar.Cell NearestCell => CurrentGrid?.GetCell( Position );

	public virtual async Task<bool> NavigateTo( GridAStar.Cell targetCell )
	{
		if ( targetCell == null ) return false;
		if ( targetCell == NearestCell ) return false;
		if ( CurrentGrid == null ) return false;

		var computedPath = await CurrentGrid.ComputePathAsync( NearestCell, targetCell, null );

		if ( computedPath == null || computedPath.Length < 1 ) return false;

		currentPath = computedPath;
		currentPathIndex = 0;
		HasArrivedDestination = false;
		targetPathCell = lastPathCell;

		return true;
	}

	public virtual async Task<bool> NavigateTo( BaseEntity target )
	{
		if ( CurrentGrid == null ) return false;

		var targetPosition = FindBestTargetPosition( target );

		return await NavigateTo( CurrentGrid.GetNearestCell( targetPosition, true, true ) );
	}

	public async virtual void ComputeNavigation()
	{
		if ( CurrentGrid == null ) return;

		if ( lastRetraceCheck )
		{
			if ( targetPathCell != lastPathCell ) // If the target cell is not the current navpath's last cell, retrace path
				await NavigateTo( targetPathCell );

			if ( IsFollowingPath && Position.DistanceSquared( currentPathCell.Position ) > (CurrentGrid.CellSize * 1.42f) * (CurrentGrid.CellSize * 1.42f) ) // Or if you strayed away from the path too far
				await NavigateTo( targetPathCell );

			lastRetraceCheck = PathRetraceFrequency;
		}

		if ( !IsFollowingPath )
		{
			Direction = Vector3.Zero;
			return;
		}

		for ( int i = 0; i < currentPath.Length; i++ )
		{
			//currentPath[i].Draw( Color.White, Time.Delta );
			//DebugOverlay.Text( i.ToString(), currentPath[i].Position, duration: Time.Delta );
		}
		Direction = (nextPathCell.Position - Position).Normal;

		if ( Position.DistanceSquared( nextPathCell.Position ) <= (CurrentGrid.CellSize / 2 + CurrentGrid.StepSize) * (CurrentGrid.CellSize / 2 + CurrentGrid.StepSize) )
			currentPathIndex++;

		if ( currentPathIndex >= currentPath.Length || currentPathCell == targetPathCell )
		{
			HasArrivedDestination = true;
			currentPathIndex = -1;
		}
	}
}
