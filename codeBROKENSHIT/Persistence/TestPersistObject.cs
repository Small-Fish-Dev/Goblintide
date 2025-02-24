namespace GoblinGame.Persistence;

[PersistType]
public class TestPersistEntity : Entity
{
	[PersistProperty]
	public int TestInteger { get; set; } = 100;

	[PersistProperty]
	public string TestProperty { get; set; }
}


[PersistType]
public class TestPersistObject
{
	public TestPersistObject()
	{
		PersistenceManager.Register( this );
	}

	~TestPersistObject()
	{
		PersistenceManager.Unregister( this );
	}

	[PersistProperty]
	public string TestProperty { get; set; }

	[PersistProperty]
	public long TestLong { get; set; } = 12321354242;

	[ConCmd.Server( "test_save" )]
	public static async void test()
	{
		var first = new TestPersistEntity()
		{
			TestProperty = "69"
		};

		var second = new TestPersistObject()
		{
			TestProperty = "ermm"
		};

		PersistenceManager.Save();

		first.Delete();

		PersistenceManager.Unregister( second );
		second = null;
	}

	[ConCmd.Server( "test_load" )]
	public static async void test2()
	{
		PersistenceManager.Load();
	}
}


