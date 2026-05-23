---
title: "Component Versioning"
icon: "🔢"
created: 2024-01-09
updated: 2024-11-07
---

# Component Versioning

You can define upgraders for components. Useful when making a breaking change to a component such as property renaming, or new data structures.

An example component upgrader is shown below:

```csharp
public sealed class MyComponent : Component
{
    // [Property] public string StringProperty { get; set; }
    // [Property] public string NewStringProperty { get; set; }
	[Property] public string[] StringPropertyArray { get; set; }

	public override int ComponentVersion => 2;
 
 	/// <summary>
	/// Defines the first upgrade for MyComponent, which deletes StringProperty and replaces it with NewStringProperty.
	/// </summary>
	/// <param name="json"></param>
	[JsonUpgrader( typeof( MyComponent ), 1 )]
	private static void StringPropertyUpgrader( JsonObject json )
	{
		json.Remove( "StringProperty", out var newNode );
		json["NewStringProperty"] = newNode;
	}
 
 	/// <summary>
	/// Defines the second upgrade for MyComponent, which deletes NewStringProperty and replaces it with an array of strings, StringPropertyArray.
	/// </summary>
	/// <param name="json"></param>
	[JsonUpgrader( typeof( MyComponent ), 2 )]
	private static void StringPropertyIntoArray( JsonObject json )
	{
		// Deletes NewStringProperty
		json.Remove( "NewStringProperty", out var newNode );

		// Creates a JsonArray object which we're going to put our old property inside of.
		var jsonArray = new JsonArray
		{
			newNode
		};

		json["StringPropertyArray"] = jsonArray;
	}
}
```


Your component has a property called `ComponentVersion`, which by default is set to 0.  Make sure to set it to your newest version when you add an upgrader:

```csharp
public sealed class MyComponent : Component
{
	public override int ComponentVersion => 2;
}
```


Upgrades are chained, so if you're a few versions out of date, it'll run every upgrade that has been missed.
