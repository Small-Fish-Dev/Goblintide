namespace GameJam;

[Prefab]
public partial class NamingComponent : CharacterComponent
{
	[Prefab]
	public Names.Type Type { get; set; }

	public override void Spawn()
	{
		if ( !Game.IsServer )
			return;
		
		Entity.DisplayName = Names.Create( Type );
	}
}
