---
title: "Cheat Sheet"
icon: "🐏"
created: 2024-08-06
updated: 2025-06-16
---

# Cheat Sheet

Sometimes you know what you're looking for, but you don't know where it is.


## Debugging

| Name | Code |
|------|------|
| Logging to console | `Log.Info( $"Hello {username}" );` |
| Drawing to screen | `DebugOverlay.ScreenText( new Vector2( 50, 50 ), "Hello" );` |
| Asserting | `Assert.NotNull( obj, "Object was null!" )` |


## Transforms

| Name | Code |
|------|------|
| Get GameObject Position | `var p = go.WorldPosition;` |
| Set GameObject Position | `go.WorldPosition = new Vector3( 10, 0, 0 );` |
| Get Local Position | `var p = go.LocalPosition;` |


## GameObjects

| Name | Code |
|------|------|
| Find by name | `Scene.Directory.FindByName( "Cube" ).First();` |
| Find by Guid | `Scene.Directory.FindByGuid( guid );` |
| Creating | `var go = new GameObject();` |
| Deleting | `go.Destroy()` |
| Disabling | `go.Enabled = false;` |
| Duplicating | `var newGo = go.Clone();` |
| Adding a Tag | `go.Tags.Add( "player" );` |
| Iterate Children | `foreach( var child in go.Children )` |
| Deleted Check | `if ( go.IsValid() )` |


## Components

| Name | Code |
|------|------|
| Add component | `var c = go.AddComponent<ModelRenderer>();` |
| Remove component | `c.Destroy()` |
| Disabling | `c.Enabled = false;` |
| Get GameObject | `var go = c.GameObject;` |
| Get Component | `var c = go.GetComponent<ModelRenderer>();` |
| Get or Add | `var c = go.GetOrAddComponent<ModelRenderer>();` |
| Iterate | `foreach ( var c in go.Components.GetAll() )` |
| Deleted check | `if ( c.IsValid() )` |
| Get all active | `foreach ( var c in Scene.GetAll<CameraComponent>() )`   |
