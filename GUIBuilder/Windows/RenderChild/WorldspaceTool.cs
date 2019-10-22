﻿/*
 * WorldspaceTool.cs
 *
 * Render window worldspace child tool window.
 *
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace GUIBuilder.Windows.RenderChild
{
    
    /// <summary>
    /// Description of WorldspaceTool.
    /// </summary>
    public partial class WorldspaceTool : Form, GodObject.XmlConfig.IXmlConfiguration
    {
        
        const string XmlLocation = "Location";
        const string XmlSize = "Size";
        bool onLoadComplete = false;
        
        public GodObject.XmlConfig.IXmlConfiguration XmlParent { get { return GodObject.Windows.GetRenderWindow( false ) as GodObject.XmlConfig.IXmlConfiguration; } }
        
        public string XmlKey { get { return this.Text; } }
        
        public string XmlPath { get{ return GodObject.XmlConfig.XmlPathTo( this ); } }
        
        Engine.Plugin.Forms.Worldspace _SelectedWorldspace = null;
        
        public WorldspaceTool()
        {
            InitializeComponent();
            ResetGUIElements();
        }
        
        void OnFormLoad( object sender, EventArgs e )
        {
            //DebugLog.Write( "GUIBuilder.RenderWindowForm.OnFormLoad() :: Start" );
            this.Translate( true );
            
            this.Location = GodObject.XmlConfig.ReadPoint( XmlPath, XmlLocation, this.Location );
            onLoadComplete = true;
        }
        
        public void SetEnableState( bool enabled )
        {
            if( this.InvokeRequired )
            {
                this.Invoke( (Action)delegate() { SetEnableState( enabled ); }, null );
                return;
            }
            pnWindow.Enabled = enabled;
        }
        
        void OverrideSize( Size size )
        {
            this.Size = size;
        }
        
        void OnActivated( object sender, EventArgs e )
        {
            OverrideSize( this.MaximumSize );
        }
        
        void OnDeactivate( object sender, EventArgs e )
        {
            OverrideSize( this.MinimumSize );
        }
        
        void OnFormMove( object sender, EventArgs e )
        {
            if( !onLoadComplete )
                return;
            GodObject.XmlConfig.WritePoint( XmlPath, XmlLocation, this.Location, true );
        }
        
        public void ResetGUIElements()
        {
            //DebugLog.Write( "GUIBuilder.RenderWindowForm.ResetGUIElements()" );
            // Worldspace controls
            tbWorldspaceFormID.Clear();
            tbWorldspaceEditorID.Clear();
            tbWorldspaceGridBottomX.Clear();
            tbWorldspaceGridBottomY.Clear();
            tbWorldspaceGridTopX.Clear();
            tbWorldspaceGridTopY.Clear();
            tbWorldspaceHeightmapTexture.Clear();
            tbWorldspaceWaterHeightsTexture.Clear();
            tbWorldspaceMapHeightMax.Clear();
            tbWorldspaceMapHeightMin.Clear();
        }
        
        void UpdateGUIElements()
        {
            ResetGUIElements();
            var rw = GodObject.Windows.GetRenderWindow( false );
            
            var worldspace = _SelectedWorldspace;
            if( worldspace != null )
            {
                var poolEntry = worldspace.PoolEntry;
                var mapData = worldspace.MapData;
                tbWorldspaceFormID.Text = worldspace.GetFormID( Engine.Plugin.TargetHandle.WorkingOrLastFullRequired ).ToString( "X8" );
                tbWorldspaceEditorID.Text = worldspace.GetEditorID( Engine.Plugin.TargetHandle.WorkingOrLastFullRequired );
                tbWorldspaceMapHeightMax.Text = poolEntry.MaxHeight.ToString( "n6" );
                tbWorldspaceMapHeightMin.Text = poolEntry.MinHeight.ToString( "n6" );
                var cellNW = mapData.GetCellNW( Engine.Plugin.TargetHandle.WorkingOrLastFullRequired );
                var cellSE = mapData.GetCellSE( Engine.Plugin.TargetHandle.WorkingOrLastFullRequired );
                tbWorldspaceGridTopX.Text = cellNW.X.ToString();
                tbWorldspaceGridTopY.Text = cellNW.Y.ToString();
                tbWorldspaceGridBottomX.Text = cellSE.X.ToString();
                tbWorldspaceGridBottomY.Text = cellSE.Y.ToString();
                tbWorldspaceHeightmapTexture.Text = poolEntry.LandHeights_Texture_File;
                tbWorldspaceWaterHeightsTexture.Text = poolEntry.WaterHeights_Texture_File;
            }
            rw.UpdateSettlementObjectChildWindowContentsForWorldspace( worldspace );
            rw.TryUpdateRenderWindow( true );
        }
        
        #region Sync Objects
        
        public List<Engine.Plugin.Forms.Worldspace> SyncObjects
        {
            get { return lvWorldspaces.SyncObjects; }
            set { lvWorldspaces.SyncObjects = value; }
        }
        
        public Engine.Plugin.Forms.Worldspace SelectedWorldspace
        {
            get{ return _SelectedWorldspace; }
        }
        
        void lvWorldspacesItemSelectionChanged( object sender, EventArgs e )
        {
            _SelectedWorldspace = null;
            var lie = e as ListViewItemSelectionChangedEventArgs;
            if( lie != null )
                _SelectedWorldspace = lvWorldspaces.SyncObjectFromListViewItem( lie.Item );
            UpdateGUIElements();
        }
        
        #endregion
        
        #region Override (Ignore) Close Button
        
        // Supress the close button on the render windows tool windows, close with the render window.
        // https://stackoverflow.com/questions/13247629/disabling-a-windows-form-closing-button
        const int CP_NOCLOSE_BUTTON = 0x200;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams mdiCp = base.CreateParams;
                mdiCp.ClassStyle = mdiCp.ClassStyle | CP_NOCLOSE_BUTTON;
                return mdiCp;
            }
        }
        
       #endregion
        
    }
}
