using Editor;
using Sandbox;
using System.Collections.Generic;
using System.Linq;

public class ClothesList : ListView
{
	public static ClothesList Instance { get; private set; }

	ClothingDresser Dresser;

	public ClothesList( Widget parent, ClothingDresser dresser, IEnumerable<Clothing> clothingFilter ) : base( parent )
	{
		Instance = this;

		ItemContextMenu = ShowItemContext;
		ItemClicked = OnItemClicked;
		Margin = 8;
		ItemSpacing = 4;
		MinimumHeight = 500;

		Dresser = dresser;

		ItemSize = new Vector2( 78, 88 + 12 );

		BuildItems( clothingFilter.Where( x => x.Title.Contains( ClothingDresserEditor.SearchQuery, System.StringComparison.OrdinalIgnoreCase ) ).Cast<object>() );
	}

	protected void OnItemClicked( object obj )
	{
		if ( obj is not Clothing entry ) return;

		var citizen = Dresser.Components.Get<SkinnedModelRenderer>();
		var asset = AssetSystem.FindByPath( entry.ResourcePath );

		if ( Dresser.EnabledClothing.ContainsValue( entry ) )
		{
			var clothingEntry = Dresser.EnabledClothing.Where( x => x.Value == entry ).FirstOrDefault();

			clothingEntry.Key.Destroy();
			Dresser.EnabledClothing.Remove( clothingEntry.Key );
			return;
		}

		if ( entry.SlotsUnder == Clothing.Slots.Skin )
		{
			if ( entry.SkinMaterial != null && entry.EyesMaterial != null )
			{
				citizen.SetMaterialOverride( Material.Load( entry.SkinMaterial ), "skin" );
				citizen.SetMaterialOverride( Material.Load( entry.EyesMaterial ), "eyes" );
			}

			return;
		}

		var clonedClothing = Dresser.Scene.CreateObject();
		clonedClothing.Parent = Dresser.GameObject;
		clonedClothing.Name = $"Clothing - {asset.Name}";

		var cloth = clonedClothing.Components.Create<SkinnedModelRenderer>();
		cloth.Model = Model.Load( entry.Model );

		cloth.BoneMergeTarget = citizen;
		cloth.Tags.Add( "clothing" );

		//Log.Info( $"{cloth.GameObject.Id}, {entry.Title}" );
		Dresser?.EnabledClothing.Add( cloth.GameObject, entry );

	}

	private void ShowItemContext( object obj )
	{
		if ( obj is not Clothing entry ) return;

		var m = new Menu();

		if ( Dresser.EnabledClothing.ContainsValue( entry ) )
		{
			m.AddOption( "Remove Clothing", "checkroom", () =>
			{
				var clothingEntry = Dresser.EnabledClothing.Where( x => x.Value == entry ).FirstOrDefault();

				clothingEntry.Key.Destroy();
				Dresser.EnabledClothing.Remove( clothingEntry.Key );
			} );
		}

		m.AddOption( "Open In Editor", "edit", () =>
		{
			var asset = AssetSystem.FindByPath( entry.ResourcePath );
			asset?.OpenInEditor();
		} );

		m.OpenAt( Editor.Application.CursorPosition );
	}

	public void BuildItems( IEnumerable<object> objects )
	{
		SetItems( objects );
	}

	protected override void PaintItem( VirtualWidget item )
	{
		var rect = item.Rect.Shrink( 0, 0, 0, 15 );

		if ( item.Object is not Clothing clothing )
			return;

		var asset = AssetSystem.FindByPath( clothing.ResourcePath );

		if ( asset is null )
		{
			Paint.SetDefaultFont();
			Paint.SetPen( Color.Red );
			Paint.DrawText( item.Rect.Shrink( 2 ), "<ERROR>", TextFlag.Center );
			return;
		}

		if ( Dresser.EnabledClothing.ContainsValue( clothing ) )
		{
			Paint.SetBrush( Theme.Green.WithAlpha( 0.5f ) );
			Paint.ClearPen();
			Paint.DrawRect( item.Rect, 4 );
		}

		if ( Paint.HasMouseOver )
		{
			Paint.SetBrush( Theme.Blue.WithAlpha( item.Selected ? 0.5f : 0.2f ) );
			Paint.ClearPen();
			Paint.DrawRect( item.Rect, 4 );
		}

		var pixmap = asset.GetAssetThumb();

		Paint.Draw( rect.Shrink( 0.1f ), pixmap );

		Paint.SetDefaultFont( 9 );

		Paint.SetPen( Color.White );
		Paint.DrawText( item.Rect.Shrink( 1 ), clothing.Title, TextFlag.CenterBottom );
	}

	protected override void OnPaint()
	{
		Paint.ClearPen();
		Paint.SetBrush( Theme.ControlBackground );
		Paint.DrawRect( LocalRect, 4 );

		base.OnPaint();
	}
}
