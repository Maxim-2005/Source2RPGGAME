---
title: "ISceneStartup"
icon: "🚪"
created: 2024-09-27
updated: 2024-09-27
---

# ISceneStartup

This event interface allows you to listen to Scene startup events. These events are run in three places:


1. In editor, when pressing play
2. In game, when loading a game
3. In game, when joining a server


```csharp
public interface ISceneStartup : ISceneEvent<ISceneStartup>
{
	void OnHostPreInitialize( SceneFile scene );
	void OnHostInitialize();
	void OnClientInitialize();
}
```


## OnHostPreInitialize

Called before the scene is loaded on the host. The scene is empty at this point.

Note that you'll only really ever see this called on GameObjectSystems. This is because at this point no Components exist in the scene.


## OnHostInitialize

Called after the scene is loaded on the host. You can now access and modify all of the GameObjects and Components in the Scene.



:::success
This is a good place for the host to spawn common things. For example, if you have a common Camera prefab, this is a good place to spawn it.

:::


## OnClientInitialize

Called after the scene is loaded, on both the host and client. This won't be called on a Dedicated Server.


:::success
This is a good place to spawn clientside only things.

Don't forget to mark these things as not networked - because otherwise if your client is also the lobby host, the scene will be snapshotted and sent to clients, along with whatever you spawn here.

:::



:::info
We're using the term host here, which can be easily misinterpreted. By host we are referring to the computer in charge of the game. This could be the player in a singleplayer game, a dedicated server host, or a host in a lobby. Basically anyone but a client connected to a server.

:::


# Examples

```csharp
public sealed class MyGameManager : GameObjectSystem<GameManager>, ISceneStartup
{
	public MyGameManager( Scene scene ) : base( scene )
	{
	}

	void ISceneStartup.OnHostInitialize()
	{
		//
		// We don't have a menu, but if we did we could put something in the menu
		// scenes that we'd now be able to detect, and skip doing the stuff below.
		//

		//
		// Spawn the engine scene.
		// This scene is sent to clients when they join.
		//
		var slo = new SceneLoadOptions();
		slo.IsAdditive = true;
		slo.SetScene( "scenes/engine.scene" );
		Scene.Load( slo );

		// If we're not hosting a lobby, start hosting one
		// so that people can join this game.
		Networking.CreateLobby();
	}
}
```
