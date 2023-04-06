namespace GameJam;

[Prefab]
public partial class TentComponent : StructureComponent
{
	public override void Initialize() 
	{
		GameMgr.MaxArmySize += 10;
	}

	public override void OnDestroy()
	{
		GameMgr.MaxArmySize -= 10;
	}
}
