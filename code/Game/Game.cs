global using Sandbox;
global using System;
global using Editor;
global using System.Collections.Generic;
global using System.Linq;
global using Sandbox.Utility;
global using System.Threading.Tasks;
global using Sandbox.Component;
using GameJam.UpgradeSystem;
using GameJam.Props.Collectable;
using System.Net.Http.Headers;

namespace GameJam;

// TODO(gio): rename GameMgr when we have an actual idea! 
public partial class GameMgr : GameManager
{
	/// <summary>
	/// The singleton for GameManager.
	/// </summary>
	public static GameMgr Instance { get; private set; }
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

	public GameMgr()
	{
		Instance = this;
		Game.TickRate = 15;

		Event.Run( nameof(GameEvents.Initialize) );

		if ( Game.IsClient )
		{
			GameMgr.Music.Stop();
			GameMgr.Music = Sound.FromScreen( "sounds/music/village_song.sound" );
		}

		WorldMapHost.Start();
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
