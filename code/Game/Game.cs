global using Sandbox;
global using System;
global using Editor;
global using System.Collections.Generic;
global using System.Linq;
global using Sandbox.Utility;
global using System.Threading.Tasks;
global using Sandbox.Component;
using GoblinGame.UpgradeSystem;
using GoblinGame.Props.Collectable;
using System.Net.Http.Headers;

namespace GoblinGame;

// TODO(gio): rename Goblintide when we have an actual idea! 
public partial class Goblintide : GameManager
{
	/// <summary>
	/// The singleton for GameManager.
	/// </summary>
	public static Goblintide Instance { get; private set; }
	public static bool Tutorial { get; set; } = true;

	/// <summary>
	/// The game's current Lord.
	/// </summary>
	public static Lord Lord
	{
		get => Instance.lord;
		set
		{
			Instance.lord = value;
		}
	}
	/// <summary>
	 /// The game's current Town.
	 /// </summary>
	public static Town CurrentTown
	{
		get => Instance.currentTown;
		set
		{
			Instance.currentTown = value;
		}
	}
	[Net] private Lord lord { get; set; }
	[Net] private Town currentTown { get; set; }

	public static readonly Dictionary<string, Model> PrecachedModels = new()
	{
		{ "models/houses/house_a.vmdl", Model.Load( "models/houses/house_a.vmdl" ) },
		{ "models/houses/house_b.vmdl", Model.Load( "models/houses/house_b.vmdl" ) },
		{ "models/houses/house_c.vmdl", Model.Load( "models/houses/house_c.vmdl" ) },
		{ "models/houses/house_d.vmdl", Model.Load( "models/houses/house_d.vmdl" ) },
		{ "models/houses/tent_a.vmdl", Model.Load( "models/houses/tent_a.vmdl" ) },
		{ "models/houses/tent_b.vmdl", Model.Load( "models/houses/tent_b.vmdl" ) },
		{ "models/containers/barrel/barrel.vmdl", Model.Load( "models/containers/barrel/barrel.vmdl" ) },
		{ "models/containers/bigcrate/big_crate.vmdl", Model.Load( "models/containers/bigcrate/big_crate.vmdl" ) },
		{ "models/containers/box/box.vmdl", Model.Load( "models/containers/box/box.vmdl" ) },
		{ "models/containers/smallcrate/smallcrate.vmdl", Model.Load( "models/containers/smallcrate/smallcrate.vmdl" ) },
		{ "models/fence/fence.vmdl", Model.Load( "models/fence/fence.vmdl" ) },
		{ "models/logwall/logwall.vmdl", Model.Load( "models/logwall/logwall.vmdl" ) },
		{ "models/trees/shitty_pine_tree2.vmdl", Model.Load( "models/trees/shitty_pine_tree2.vmdl" ) },
		{ "models/stand/stand.vmdl", Model.Load( "models/stand/stand.vmdl" ) },
		{ "models/waggon/waggon.vmdl", Model.Load( "models/waggon/waggon.vmdl" ) }
	};

	public static Dictionary<string, float>[] WeaponsList = new Dictionary<string, float>[6]
{
		new Dictionary<string, float>()
		{
			{ "prefabs/items/club.prefab", 1f },
		},
		new Dictionary<string, float>()
		{
			{ "prefabs/items/club.prefab", 0.75f },
			{ "prefabs/items/spear.prefab", 0.25f },
		},
		new Dictionary<string, float>()
		{
			{ "prefabs/items/club.prefab", 0.5f },
			{ "prefabs/items/spear.prefab", 0.5f },
		},
		new Dictionary<string, float>()
		{
			{ "prefabs/items/club.prefab", 0.45f },
			{ "prefabs/items/spear.prefab", 0.45f },
			{ "prefabs/items/staff.prefab", 0.1f },
		},
		new Dictionary<string, float>()
		{
			{ "prefabs/items/club.prefab", 0.4f },
			{ "prefabs/items/spear.prefab", 0.4f },
			{ "prefabs/items/staff.prefab", 0.2f },
		},
		new Dictionary<string, float>()
		{
			{ "prefabs/items/club.prefab", 0.34f },
			{ "prefabs/items/spear.prefab", 0.33f },
			{ "prefabs/items/staff.prefab", 0.33f },
		}
};

	public Goblintide()
	{
		Instance = this;
		Game.TickRate = 15;

		Event.Run( nameof(GameEvents.Initialize) );

		if ( Game.IsClient )
		{
			Goblintide.Music.Stop();
			Goblintide.Music = Sound.FromScreen( "sounds/music/village_song.sound" );
		}

		WorldMapHost.Start();
	}

	public TimeUntil nextTutorial = 10000f;
	public List<string> tutorialPhrases = new List<string>()
	{
		"Barely escaping with your life, you're now forced to build back your goblin army...",
		$"You'll need resources to build your settlement and upgrade yourself.",
		$"Travelling takes energy, and energy takes time to restore. So make sure to upgrade it!",
		$"Open your map and select a tent with a low number to begin your raid!"
	};

	public static void DoTutorial()
	{
		Goblintide.Instance.nextTutorial = 1f;
	}

	[Event.Tick.Server]
	public static void ComputeTutorial()
	{
		if ( !Goblintide.Tutorial ) return;

		if ( Goblintide.Instance.nextTutorial )
		{
			if ( Goblintide.Instance.tutorialPhrases.Count == 0 )
			{
				Goblintide.Instance.nextTutorial = 999999f;
				return;
			}
			var curPhrase = Goblintide.Instance.tutorialPhrases.First();
			var duration = curPhrase.Length / 16f;
			GameplayHints.Send( To.Everyone, curPhrase, duration );
			if ( curPhrase.StartsWith( "Barely" ) )
				Sound.FromScreen( "sounds/tutorial/tutorial1.sound" );
			if ( curPhrase.StartsWith( "You'll" ) )
				Sound.FromScreen( "sounds/tutorial/tutorial2.sound" );
			if ( curPhrase.StartsWith( "Travelling" ) )
				Sound.FromScreen( "sounds/tutorial/tutorial3.sound" );
			if ( curPhrase.StartsWith( "Open your" ) )
				Sound.FromScreen( "sounds/tutorial/tutorial4.sound" );
			if ( curPhrase.StartsWith( "Sneak" ) )
				Sound.FromScreen( "sounds/tutorial/tutorial5.sound" );
			if ( curPhrase.StartsWith( "The goblins" ) )
				Sound.FromScreen( "sounds/tutorial/tutorial6.sound" );
			if ( curPhrase.StartsWith( "Hold Right" ) )
				Sound.FromScreen( "sounds/tutorial/tutorial8.sound" );
			if ( curPhrase.StartsWith( "You took" ) )
				Sound.FromScreen( "sounds/tutorial/tutorial9.sound" );
			if ( curPhrase.StartsWith( "Make sure" ) )
				Sound.FromScreen( "sounds/tutorial/tutorial10.sound" );

			Goblintide.Instance.tutorialPhrases.RemoveAt( 0 );
			if ( Goblintide.Instance.tutorialPhrases.Count == 0 )
			{
				Goblintide.Instance.nextTutorial = 999999f;
				return;
			}
			else
				Goblintide.Instance.nextTutorial = duration + 0.3f;

		}
	}

	public override void ClientJoined( IClient client )
	{
		// Hard code player limit to 1.
		if ( Lord != null )
		{
			client.Kick();
			return;
		}

		base.ClientJoined( client );

		var pawn = new Lord();
		client.Pawn = pawn;
		Lord = pawn;

		if ( All.OfType<SpawnPoint>().MinBy( x => Guid.NewGuid() ) is { } spawn )
		{
			var transform = spawn.Transform;
			transform.Position += Vector3.Up * 50.0f;
			pawn.Transform = transform;
		}

		// Load the save.
		SetState<VillageState>();
		LoadSave();

		SetEnergyFromLastEnergyDate();
		PlaceGoblinArmy( true );

		if ( WorldMapHost.IsEmpty )
			WorldMapHost.GenerateNew();

		// Broadcast nodes to client.
		WorldMapHost.BroadcastNodes( To.Everyone );
	}

	public override void RenderHud()
	{
		base.RenderHud();
		Event.Run( nameof( GameEvents.Render ) );
	}

	[ClientRpc]
	public static void BroadcastTrees()
	{
		Town.PlaceTrees();
	}

	[ClientRpc]
	public static void BroadcastFences()
	{
		Town.PlaceFences();
	}
}
