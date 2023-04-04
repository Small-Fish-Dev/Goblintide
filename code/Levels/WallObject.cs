namespace GameJam;

public partial class WallObject : SceneObject
{
	public float Length { get; set; } = 100f;
	public static BBox Box { get; set; } = new BBox( new Vector3( -10f ), new Vector3( 10f ) );
	internal TimeUntil checkTimer { get; set; } = 0.5f;

	public WallObject( SceneWorld sceneWorld, string model, Transform transform, float length ) : base( sceneWorld, model, transform )
	{
		Tags.Add( "nocamera" );
		Length = length;
	}

	public void Break()
	{
		if ( !this.IsValid() ) return;
		Sound.FromWorld( "sounds/physics/breaking/break_wood_plank.sound", Position );
		Particles.Create( Length > 110f ? "particles/wood_shatter_large.vpcf" : "particles/wood_shatter.vpcf", Position );
		Breakables.Break( Model, Position, Rotation, 1f, Color.White );

		var gibs = Entity.All
			.OfType<PropGib>()
			.Where( x => x.Position.DistanceSquared( Position ) <= Length * Length );

		foreach ( var gib in gibs )
		{
			gib.Velocity = Vector3.Random * 300;
			gib.Tags.Add( "nocamera" );
		}
		Town.TownFences.Remove( this );
		Delete();
	}
}
