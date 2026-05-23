---
title: "Async"
icon: "🧵"
created: 2023-12-28
updated: 2024-10-03
---

# Async

By default, async tasks in s&box run on the main thread. They operate a lot like coroutines, making them the perfect replacement.


# Using Async

To make a method asynchronous, you do it like this..

```csharp
async Task PrintSomething( float waitSeconds, string message )
{
	// wait for this amount of seconds
	await Task.DelaySeconds( waitSeconds );

	// Print it
	Log.Info( message );
}
```

Components have a special Task property with some extra helper functions (like `DelayRealtimeSeconds`).

As you can see, if a task is async, you can await it.

```csharp
async Task LerpSize( float seconds, Vector3 to, Easing.Function easer )
{
	TimeSince timeSince = 0;
	Vector3 from = WorldScale;
 
	while ( timeSince < seconds )
	{
		var size = Vector3.Lerp( from, to, easer( timeSince / seconds ) );
		WorldScale = size;
		await Task.Frame(); // wait one frame
	}
}
 
await LerpSize( 3.0f, Vector3.One * 3.3f, Easing.BounceOut );
await LerpSize( 1.0f, Vector3.One * 4.0f, Easing.EaseInOut );
await LerpSize( 1.0f, Vector3.One * 3.0f, Easing.EaseInOut );
```


# Multiple Async

Tasks can orchestrate and do multiple tasks at once. This feels like multithreading but it's not.

```csharp
async Task DoMultipleThings()
{
	// notice no await here
	Task taskOne = PrintSomething( 2.0f, "One" );
	Task taskTwo = PrintSomething( 3.0f, "Two" );

	// wait for both these tasks
	await Task.WhenAll( taskOne, taskTwo );
}
```


# Returning Values

Async Tasks can return values too. 

```csharp
async Task<string> GetKanyeQuote()
{
	string kanyeQuote = await Http.RequestStringAsync( "https://api.kanye.rest/" );

	kanyeQuote = kanyeQuote.Replace( "music", "poosic" );

	return kanyeQuote;
}

async Task PrintKanyeQuote()
{
	string quote = await GetKanyeQuote();
	Log.Info( $"KANYE SAID: {quote}" );
}
```


# Cooperating with synchronous code

This is all cool, but how do you call these async functions from your regular functions?  

```csharp
protected override void OnEnabled()
{
	// here the _ just tells the compiler that we don't care about the task
	_ = DoMultipleThings();
}
```

But what if from synchronous code you want to use the value?

```csharp
protected override void OnEnabled()
{
    // Will run async and run this Action when the task finishes
    GetKanyeQuote().ContinueWith( task => Log.Info( $"Kanye: {task.Result}" ) );
}
```

But what if I want to do it more stupidly?

```csharp
Task<string> getQuoteTask;

protected override void OnEnabled()
{
	getQuoteTask = GetKanyeQuote();
}

protected override void OnUpdate()
{
	if ( getQuoteTask is not null && getQuoteTask.IsCompletedSuccessfully )
	{
		Log.Info( $"Kanye: {getQuoteTask.Result}" );
		getQuoteTask = null;
	}
}
```


# Being Responsible

Something to be thinking of is what happens when your GameObject is destroyed or disabled while you're waiting. 

When implementing things yourself you should be considerate of this.. the async method isn't guaranteed to stop just because the GameObject or Component is gone. 


:::info
We do somewhat handle this internally, when awaiting a method in Component.Task we will automatically cancel the task if the GameObject turns invalid.

:::


# Common Errors

A common async error is letting tasks stack up. 

For example, if you have a system where a user presses space, it waits a second, then shoots a button.. you need to handle a user pressing that button multiple times during that second. You need to handle the user dying during that second.

Maybe you want to not launch a new async task if the user firing task is running. Maybe you want to cancel the firing task and start it again (use a `CancellationToken` maybe).
