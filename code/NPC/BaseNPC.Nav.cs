using Sandbox.Internal;

namespace GameJam;

public partial class BaseNPC
{
	public NavAgentHull Agent => NavAgentHull.Default;

	NavPath currentPath;
	Vector3 currentTargetPosition = 0;
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

		currentPathIndex = 0;
		IsFollowingPath = true;
		currentTargetPosition = targetPosition;

		return true;
	}

	public virtual void ComputeNavigation()
	{

		if ( !IsFollowingPath )
		{
			Direction = Vector3.Zero;
			return;
		}

		Direction = (nextPathPoint.Position - Position).Normal;

		if ( Position.DistanceSquared( nextPathPoint.Position ) >= 40f )
			currentPathIndex++;

		if ( distanceFromIdealPath >= 100f )
			NavigateTo( currentTargetPosition );

		if ( currentPathIndex >= currentPathCount )
		{
			IsFollowingPath = false;
		}

	}
}
