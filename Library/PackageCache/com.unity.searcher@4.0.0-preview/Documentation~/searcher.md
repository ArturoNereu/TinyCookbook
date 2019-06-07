# About Searcher

![GitHub Logo](/Documentation/images/quick_search.png)

Use the Searcher package to quickly search a large list of items via a popup window. For example, use Searcher to find, select, and put down a new node in a graph. The Searcher package also includes samples and tests.

# Installing Searcher

To install this package, follow the instructions in the [Package Manager documentation](https://docs.unity3d.com/Packages/com.unity.package-manager-ui@latest/index.html).

<a name="UsingPackageName"></a>
# Using Searcher

### Quick Usage Example

```csharp
void OnMouseDown( MouseDownEvent evt )
{
    var items = new List<SearcherItem>
    {
        new SearcherItem( "Books", "Description", new List<SearcherItem>()
        {
            new SearcherItem( "Dune" ),
        } )
    };
    items[0].AddChild( new SearcherItem( "Ender's Game" ) );

    SearcherWindow.Show(
        this, // this EditorWindow
        items, "Optional Title",
        item => { Debug.Log( item.name ); return /*close window?*/ true; },
        evt.mousePosition );
}
```

### Searcher Creation from Database

```csharp
var bookItems = new List<SearcherItem> { new SearcherItem( "Books" ) };
var foodItems = new List<SearcherItem> { new SearcherItem( "Foods" ) };

// Create databases.
var databaseDir = Application.dataPath + "/../Library/Searcher";
var bookDatabase = SearcherDatabase.Create( bookItems, databaseDir + "/Books" );
var foodDatabase = SearcherDatabase.Create( foodItems, databaseDir + "/Foods" );

// At a later time, load database from disk.
bookDatabase = SearcherDatabase.Load( databaseDir + "/Books" );

var searcher = new Searcher(
    new SearcherDatabase[]{ foodDatabase, bookDatabase },
    "Optional Title" );
```

### Popup Window or Create Control

```csharp
Searcher m_Searcher;

void OnMouseDown( MouseDownEvent evt ) { // Popup window...
   SearcherWindow.Show( this, m_Searcher,
       item => { Debug.Log( item.name ); return /*close window?*/ true; },
       evt.mousePosition );
}

// ...or create SearcherControl VisualElement
void OnEnable() { // ...or create SearcherControl VisualElement
   var searcherControl = new SearcherControl();
   searcherControl.Setup( m_Searcher, item => Debug.Log( item.name ) );
   this.GetRootVisualContainer().Add( searcherControl );
}
```

### Customize the UI via `ISearcherAdapter`

```csharp
public interface ISearcherAdapter {
   VisualElement MakeItem();
   VisualElement Bind( VisualElement target, SearcherItem item,
                       ItemExpanderState expanderState, string text );
   string title { get; }
   bool hasDetailsPanel { get; }
   void DisplaySelectionDetails( VisualElement detailsPanel, SearcherItem o );
   void DisplayNoSelectionDetails( VisualElement detailsPanel );
   void InitDetailsPanel( VisualElement detailsPanel );
}

var bookDatabase = SearcherDatabase.Load( Application.dataPath + "/Books" );
var myAdapter = new MyAdapter(); // class MyAdapter : ISearcherAdapter
var searcher = new Searcher( bookDatabase, myAdapter );

```

### Enabling the Samples and Tests

Open this file in your project:
```
Packages/manifest.json
```
Add a ```testables``` list with the package name so you get something like this (makes sure to change the version string to your current version):
```json
{
    "dependencies": {
        "com.unity.searcher": "4.0.0-preview"
    },
    "testables" : [ "com.unity.searcher" ],
    "registry": "https://staging-packages.unity.com"
}
```
You should see a new top-level menu called **Searcher**.

# Technical details
## Requirements

This version of Searcher is compatible with the following versions of the Unity Editor:

* 2019.1 and later (recommended)

## Known limitations

Searcher version 1.0 includes the following known limitations:

* Only works with .Net 4.0

## Package contents

The following table indicates the main folders of the package:

|Location|Description|
|---|---|
|`Editor/Resources`|Contains images used in the UI.|
|`Editor/Searcher`|Contains Searcher source files.|
|`Samples`|Contains the samples.|
|`Tests`|Contains the tests.|

## Document revision history

|Date|Reason|
|---|---|
|Apr 25, 2018|Document created. Matches package version 1.0.|
