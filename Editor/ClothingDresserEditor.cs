

using Editor;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

[CustomEditor( typeof( ClothingDresser ) )]
public class ClothingDresserEditor : ComponentEditorWidget
{

	public static List<Clothing> AllClothing => ResourceLibrary.GetAll<Clothing>().ToList();

	public static LineEdit Filter { get; set; }

	public ClothesList ClothesList { get; set; }

	public static string SearchQuery { get; set; } = string.Empty;

	public ClothingDresserEditor( SerializedObject obj ) : base( obj )
	{
		Layout = Layout.Column();
		Layout.Margin = new( 5, 5 );


		var tabBar = new SegmentedControl( this );
		tabBar.SetSizeMode( SizeMode.Flexible, SizeMode.CanShrink );


		var row = Layout.AddRow();
		row.Add( tabBar );

		var categories = Enum.GetValues<Clothing.ClothingCategory>();

		foreach ( var category in categories )
		{
			tabBar.AddOption( category.ToString() );
		}

		tabBar.OnSelectedChanged += ( string tab ) =>
		{
			if ( tab == "None" )
			{
				ClothesList.BuildItems( AllClothing );
				return;
			}

			ClothesList.BuildItems( AllClothing.Where( x => x.Category.ToString() == tab ).Cast<object>() );
		};



		var dresser = SerializedObject.Targets.FirstOrDefault() as ClothingDresser;

		var secondRow = Layout.AddRow();

		var header = new Label( "Clothing" );
		header.SetStyles( "font-size: 13px;" );

		Filter = new LineEdit();
		Filter.SetStyles( "font-family: Poppins; font-weight: 500;" );
		Filter.PlaceholderText = "Filter..";
		Filter.FixedHeight = 30f;
		Filter.TextEdited += ( string text ) =>
		{
			if ( !string.IsNullOrEmpty( text ) )
			{
				// Log.Info( text );
				SearchQuery = text;
				ClothesList.BuildItems( AllClothing.Where( x => x.Title.Contains( SearchQuery, StringComparison.OrdinalIgnoreCase ) ).Cast<object>() );
				return;
			}

			ClothesList.BuildItems( AllClothing );
		};


		var resetClothing = new Button( "Reset Clothing" );
		resetClothing.ButtonType = "danger";
		resetClothing.FixedWidth = 112f;
		resetClothing.SetStyles( "font-size: 13px;" );

		resetClothing.Clicked = () =>
		{
			foreach ( var kv in dresser?.EnabledClothing )
			{
				kv.Key.Destroy();
			}

			dresser?.EnabledClothing.Clear();
		};

		secondRow.Margin = new( 15, 10 );
		secondRow.Add( header );
		secondRow.Add( resetClothing );

		var checkbox = new Checkbox();
		checkbox.Text = "Filter Enabled";
		checkbox.Toggled += () =>
		{
			if ( checkbox.Value )
			{
				ClothesList.BuildItems( AllClothing.Where( x => dresser.EnabledClothing.ContainsValue( x ) ).Cast<object>() );
			}
			else
			{
				ClothesList.BuildItems( AllClothing );
			}
		};

		var thirdRow = Layout.AddRow();
		thirdRow.Margin = new( 15, 0 );
		thirdRow.Add( Filter );
		thirdRow.AddStretchCell();
		thirdRow.Add( checkbox );

		ClothesList = new ClothesList( null, dresser, AllClothing );

		var tlayout = Layout.AddColumn();
		tlayout.Spacing = 8;
		tlayout.Margin = 16;
		tlayout.Add( ClothesList );
	}
}
