# BlazorRoutes [![BlazorRoutes on NuGet](https://img.shields.io/nuget/v/BlazorRoutes.svg)](https://www.nuget.org/packages/BlazorRoutes)  
This library generates source code for Blazor page routes in order to eliminate hard-coded magic strings and manual route building. It's inspired by [R4MVC](https://github.com/T4MVC/R4MVC) and [Andrew Lock's article](https://andrewlock.net/using-source-generators-to-find-all-routable-components-in-a-webassembly-app/).

## Examples

### Simple route

Instead of writing

```html
<NavLink class="nav-link" href="/counter">
```

you can now write

```html
<NavLink class="nav-link" href="@Routes.Counter()">
```

### Parameterized route

Let's add a currentCount parameter to the Counter page:

```csharp
@page "/counter/{currentCount?}"

// ...

[Parameter]
public int CurrentCount { get; set; }
```

So a code like

```html
<a href="/counter/@newCount">
```

can be written as

```html
<a href="@Routes.Counter(newCount)">
```

### Multiple route paths

You can add multiple `@page` directives to a Blazor page. For example:

```csharp
@page "/fetchdata"
@page "/fetchdata/page/{page:int}"

// ...

[Parameter]
public int? Page { get; set; }

protected override void OnParametersSet()
{
    Page ??= 1;
}
```

Now we can replace the code

```csharp
NavigationManager.NavigateTo(page > 1 ? "/fetchdata/page/" + page : "/fetchdata");
```

with

```csharp
NavigationManager.NavigateTo(page > 1 ? Routes.FetchData(page) : Routes.FetchData());
```

Unfortunately, Blazor does not support non-type route constraints yet (like min/max), so we are forced to check page number whether it is greater than one. Hopefully, this extra logic will be unnecessary in the future.

## Invariant culture parameters

The generated route parameters are converted to string with `CultureInfo.InvariantCulture`. Furthermore, boolean values are lowercase (`true`/`false`), and `DateTime` values are converted to either `2021-03-29` or `2021-03-29T15:46:30`, depending on the time part.
