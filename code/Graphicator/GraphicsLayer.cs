namespace Graphicator;

public class GraphicsLayer
{
	private int _zIndex;

	public int ZIndex
	{
		get => _zIndex;
		set
		{
			_zIndex = value;
			Context.ResortLayers();
		}
	}

	public RenderHook.Stage RenderStage = RenderHook.Stage.AfterUI;

	public GraphicsLayer()
	{
		Log.Info( $"New GraphicsLayer {this}" );
		Context.Layers.Add( this );
	}

	private readonly List<GraphicsItem> _items = new();
	public IEnumerable<GraphicsItem> Items => _items.AsReadOnly();

	public void Add( GraphicsItem item )
	{
		if ( _items.Contains( item ) )
		{
			Log.Warning( $"{item} already a child of {this}" );
			return;
		}

		_items.Add( item );
	}

	internal void Render()
	{
		foreach ( var item in _items )
		{
			item.Render();
		}
	}

	internal void Update()
	{
		foreach ( var item in _items )
		{
			item.Update();
		}
	}
}
