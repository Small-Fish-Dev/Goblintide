﻿namespace GoblinGame;

public static class WeightedList
{
	public static string RandomKey( Dictionary<string, float> weightedDictionary )
	{
		var totalWeight = 0f;

		foreach ( float weight in weightedDictionary.Values )
		{
			totalWeight += weight;
		}

		var randomValue = (float)(new Random().NextDouble() * totalWeight);

		foreach ( KeyValuePair<string, float> entry in weightedDictionary )
		{
			randomValue -= entry.Value;
			if ( randomValue <= 0 )
			{
				return entry.Key;
			}
		}

		return null;
	}

	public static int RandomKey( Dictionary<int, float> weightedDictionary )
	{
		var totalWeight = 0f;

		foreach ( float weight in weightedDictionary.Values )
		{
			totalWeight += weight;
		}

		var randomValue = (float)(new Random().NextDouble() * totalWeight);

		foreach ( KeyValuePair<int, float> entry in weightedDictionary )
		{
			randomValue -= entry.Value;
			if ( randomValue <= 0 )
			{
				return entry.Key;
			}
		}

		return 0;
	}

	public static Color RandomKey( Dictionary<Color, float> weightedDictionary )
	{
		var totalWeight = 0f;

		foreach ( float weight in weightedDictionary.Values )
		{
			totalWeight += weight;
		}

		var randomValue = (float)(new Random().NextDouble() * totalWeight);

		foreach ( KeyValuePair<Color, float> entry in weightedDictionary )
		{
			randomValue -= entry.Value;
			if ( randomValue <= 0 )
			{
				return entry.Key;
			}
		}

		return null;
	}

	public static string RandomKey( Random random, Dictionary<string, float> weightedDictionary )
	{
		var totalWeight = 0f;

		foreach ( float weight in weightedDictionary.Values )
		{
			totalWeight += weight;
		}

		var randomValue = (float)(random.NextDouble() * totalWeight);

		foreach ( KeyValuePair<string, float> entry in weightedDictionary )
		{
			randomValue -= entry.Value;
			if ( randomValue <= 0 )
			{
				return entry.Key;
			}
		}

		return null;
	}
}
