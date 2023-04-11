namespace GoblinGame;

public partial class BaseCharacter : BaseEntity
{
	[Net, Prefab]
	public string DisplayName { get; set; }

	public virtual float CollisionWidth { get; set; } = 20f;
	public virtual float CollisionHeight { get; set; } = 40f;
	public virtual BBox CollisionBox => new( new Vector3( -CollisionWidth / 2f, -CollisionWidth / 2f, 0f ), new Vector3( CollisionWidth / 2f, CollisionWidth / 2f, CollisionHeight ) );

	[Net] public float Height {get; set;} = 0f;

	public override float GetWidth() => CollisionWidth;
	public override float GetHeight() => CollisionHeight;

	public override bool BlockNav { get; set; } = false;
	[Net] bool disabled { get; set; } = false;
	[Net] public bool Dead { get; set; } = false;
	public bool Disabled
	{
		get { return disabled; }
		set
		{
			disabled = value;

			EnableDrawing = !disabled;
			EnableAllCollisions = !disabled;
			UseAnimGraph = !disabled;
			EnableShadowCasting = !disabled;
			Transmit = disabled ? TransmitType.Never : TransmitType.Pvs;

			foreach ( var child in Children )
			{
				child.EnableDrawing = !disabled;
				child.EnableShadowCasting = !disabled;
				child.Transmit = disabled ? TransmitType.Never : TransmitType.Pvs;
			}
		}
	}

	public BaseCharacter() {}

	public override void Spawn()
	{
		SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, CollisionBox.Mins, CollisionBox.Maxs );

		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableTouch = true;

		Tags.Add( "Pushable" );
		Tags.Add( Faction.ToString() );

		Height = GetHeight();

		if ( Game.IsServer )
			foreach ( var component in Components.GetAll<CharacterComponent>() )
				component.Spawn();
	}

	public override void ClientSpawn()
	{
		foreach ( var component in Components.GetAll<CharacterComponent>() )
			component.Spawn();
	}
}
