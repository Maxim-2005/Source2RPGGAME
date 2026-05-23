---
title: "ITemporaryEffect"
icon: "auto_awesome"
created: 2024-11-07
updated: 2025-04-04
---

# ITemporaryEffect

The interface Component.ITemporaryEffect is used on a component that comes to an end.


```csharp
	public interface ITemporaryEffect
	{
		bool IsActive { get; }
	}
```


This is implemented on ParticleEffect and ParticleEmitter,  and returns true while they're emitting and have particles that are still alive.  It is also implemented on SoundPoint and returns true while the sound is playing.


The intended usage of this interface is to allow you to check a temporary GameObject to see if it's finished being useful - in a generic way. This is how the TemporaryEffect component works.
