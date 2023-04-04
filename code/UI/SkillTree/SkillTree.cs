using GameJam.UpgradeSystem;
using Sandbox.UI;

namespace GameJam.UI;

public partial class SkillTree
{
	private void GenerateUpgrades( Panel panel )
	{
		return;
		
		foreach ( var upgrade in Upgrade.All )
		{
			var actor = new UpgradeActor( upgrade );
			panel.AddChild( actor );
		}

		var actors = Descendants.OfType<UpgradeActor>().ToList();

		foreach ( var actor in actors )
		{
			foreach ( var dependencyId in actor.Upgrade.Dependencies )
			{
				var dependencyActor = actors.SingleOrDefault( v => v.Upgrade.Identifier == dependencyId );
				if ( dependencyActor == null )
					throw new Exception( $"Unknown or no existing actor for dependency {dependencyId}" );
				actor.Dependencies.Add( dependencyActor );
			}
		}

		// Set initial selected actor
		Select( actors.FirstOrDefault() );
	}
}
