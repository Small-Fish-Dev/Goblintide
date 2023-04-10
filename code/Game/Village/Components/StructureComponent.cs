namespace GoblinGame;

public partial class StructureComponent : EntityComponent<BaseStructure>
{
	/// <summary>
	/// The ItemPrefab entity this component is attached to.
	/// </summary>
	public BaseStructure Structure => Entity;

	/// <summary>
	/// Called when the BaseStructure constructor is called.
	/// </summary>
	public virtual void Initialize() { }

	/// <summary>
	/// Called when the BaseStructure is destroyed.
	/// </summary>
	public virtual void OnDestroy() { }
}
