namespace GameJam;

[Prefab, Category( "Prop" )]
public partial class BaseProp : BaseEntity
{

	[Prefab, Category( "Stats" )]
	public override float HitPoints { get; set; } = 0.5f;

	public BaseProp() { }

	public override void Spawn()
	{
		base.Spawn();

		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );

		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableTouch = true;

		Tags.Add( Faction.ToString() );
	}

	public override void Kill()
	{
		var result = new Breakables.Result();
		Breakables.Break( this, result );

		base.Kill();
	}
}
