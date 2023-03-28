using Sandbox.Internal;

namespace GameJam;

public partial class BaseNPC
{
	public NavAgentHull Agent => NavAgentHull.Default;

	NavPath currentPath;
	public Vector3 CurrentTargetPosition = 0;
	int currentPathIndex { get; set; } = 0;
	public bool IsFollowingPath { get; set; } = false;
	NavPathSegment latestPathPoint => currentPath.Segments[currentPathIndex];
	int currentPathCount => currentPath.Count - 1;
	NavPathSegment nextPathPoint => currentPathIndex < currentPathCount ? currentPath.Segments[currentPathIndex + 1] : latestPathPoint;
	TimeSince pathLastCalculated => currentPath.Age;
	Line currentPathLine => new Line( latestPathPoint.Position, nextPathPoint.Position );
	float distanceFromIdealPath => currentPathLine.SqrDistance( Position );

	public virtual bool NavigateTo( Vector3 targetPosition, bool acceptIncomplete = false )
	{

		var pathSettings = NavMesh.PathBuilder( Position )
			.WithAgentHull( Agent );

		if ( acceptIncomplete )
			pathSettings.WithPartialPaths();

		var pathBuilt = pathSettings.Build( targetPosition );

		if ( pathBuilt == null || pathBuilt.Segments.Count == 0 ) return false;

		currentPath = pathBuilt;
		currentPathIndex = 0;
		IsFollowingPath = true;
		CurrentTargetPosition = targetPosition;

		return true;
	}

	public virtual bool NavigateTo( BaseEntity target )
	{
		var targetPosition = FindBestTargetPosition( target );

		return NavigateTo( targetPosition, true );
	}

	public virtual void ComputeNavigation()
	{

		if ( !IsFollowingPath )
		{
			Direction = Vector3.Zero;
			return;
		}

		Direction = (nextPathPoint.Position - Position).Normal;

		if ( Position.DistanceSquared( nextPathPoint.Position ) <= 40f )
			currentPathIndex++;

		if ( distanceFromIdealPath >= 50f )
			NavigateTo( CurrentTargetPosition );

		if ( currentPathIndex >= currentPathCount )
		{
			IsFollowingPath = false;
		}

	}
}
