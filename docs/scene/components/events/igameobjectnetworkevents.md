---
title: "IGameObjectNetworkEvents"
icon: "🚪"
created: 2024-10-01
updated: 2024-10-01
---

# IGameObjectNetworkEvents

Allows a GameObject's Components to listen to changes in ownership state. 


```csharp
public interface IGameObjectNetworkEvents : ISceneEvent<IGameObjectNetworkEvents>
{
	/// <summary>
	/// Called when the owner of a network GameObject is changed
	/// </summary>
	void NetworkOwnerChanged( Connection newOwner, Connection previousOwner ) { }

	/// <summary>
	/// We have become the controller of this object, we are no longer a proxy
	/// </summary>
	void StartControl() { }

	/// <summary>
	/// This object has become a proxy, controlled by someone else
	/// </summary>
	void StopControl() { }
}
```



:::success
This event is only targetted at a single GameObject - the one that is changing.

:::
