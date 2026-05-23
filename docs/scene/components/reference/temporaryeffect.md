---
title: "TemporaryEffect"
icon: "💨"
created: 2025-04-04
updated: 2025-04-04
---

# TemporaryEffect

This component is generally added to Prefabs that contain ParticleEffects and Sounds. It will automatically delete the GameObject after the particles are all gone and the sounds are done.


This uses the [ITemporaryEffect] to see if all of the child effects are done. When they are, it will delete itself. You can implement the [ITemporaryEffect] interface on your own components to become compatible.
