﻿/*
 * Cells.cs
 * 
 * Collection for CELL records, allows searching by Cell Grid in Worldspaces.
 * 
 */

using System.Collections.Generic;

using Maths;

using Engine.Plugin.Interface;

using XeLib;


namespace Engine.Plugin.Collections
{
    
    /// <summary>
    /// Description of Cell.
    /// </summary>
    public class Cells : Collection
    {
        
        const int BLOCK_SIZE = 32;
        const int SUB_BLOCK_SIZE = 8;
        
        Plugin.Forms.Cell _Persistent = null;
        Dictionary<Vector2i,Plugin.Forms.Cell> _ByGrid = null;
        
        //public Cells( Plugin.Forms.Worldspace ancestor ) : base( "CELL", ancestor ) {}
        
        //public Cells( Plugin.Forms.Worldspace ancestor ) : base( typeof( Plugin.Forms.Cell ), ancestor ) {}
        public Cells( Attributes.ClassAssociation association, IXHandle ancestor ) : base( association, ancestor ) {}
        public Cells( Attributes.ClassAssociation association ) : base( association ) {}
        
        protected override void Dispose( bool disposing )
        {
            // Base will set this, after it's disposed, so check first _then_ call it
            if( Disposed )
                return;
            base.Dispose( disposing );
            
            if( _ByGrid != null )
                _ByGrid.Clear();
            _ByGrid = null;
            
            _Persistent = null;
        }
        
        public Plugin.Forms.Cell GetByGrid( Vector2i coords )
        {
            //DebugLog.OpenIndentLevel( new [] { this.GetType().ToString(), "GetByGrid()", "coords = " + coords.ToString(), "Worldspace = " + ( Worldspace == null ? "[null]" : Worldspace.ToString() ) } );
            
            Plugin.Forms.Cell cell = null;
            
            if( !this.IsValid() )
            {
                DebugLog.WriteError( this.GetType().ToString(), "GetByGrid()", "Cell Collection is not valid!" );
                goto localReturnResult;
            }
            var wsMax = Worldspace.WorldBounds.GetMax( TargetHandle.WorkingOrLastFullRequired );
            var wsMin = Worldspace.WorldBounds.GetMin( TargetHandle.WorkingOrLastFullRequired );
            if(
                ( coords.X > wsMax.X )||( coords.X < wsMin.X )||
                ( coords.Y > wsMax.Y )||( coords.Y < wsMin.Y )
            ){
                DebugLog.WriteError( this.GetType().ToString(), "GetByGrid()", "Grid is out of bounds for the worldspace :: " + coords.ToString() + " :: " + wsMax.ToString() + " - " + wsMin.ToString() );
                goto localReturnResult;
            }
            
            // Return now it's been loaded already
            if( ( _ByGrid != null )&&( _ByGrid.TryGetValue( coords, out cell ) ) )
            {
                //DebugLog.WriteLine( "Found grid in dictionary" );
                goto localReturnResult;
            }
            
            #region Copy-pasta from XeLib source
            
            // Because PASCAL/Delphi is an incredibly slow and shitty programming language (seriously, XeLib is
            // 8MB - 8 fucking megabytes!  What the fuck is that shit???  Where it should be no more than 500KB
            // TOPs and even that is 250KB too large), the authors of XeLib have injected fake records to sub-
            // divide the CELL records in a worldspace because otherwise it would literally take hours to look
            // through ~36K records (trust me, it took almost 2 hours on my 4GHz 8-core Amd FX8350) where as any
            // other language it should and would only take 200-300ms.
            
            // Calculate which bullshit injected block and sub-block record the cell is in and load that entire
            // bullshit injected sub-block.
            
            var block = coords / BLOCK_SIZE;
            if( ( coords.X < 0 )&&( ( coords.X % BLOCK_SIZE ) != 0 ) ) block.X--;
            if( ( coords.Y < 0 )&&( ( coords.Y % BLOCK_SIZE ) != 0 ) ) block.Y--;
            var sub = coords / SUB_BLOCK_SIZE;
            if( ( coords.X < 0 )&&( ( coords.X % SUB_BLOCK_SIZE ) != 0 ) ) sub.X--;
            if( ( coords.Y < 0 )&&( ( coords.Y % SUB_BLOCK_SIZE ) != 0 ) ) sub.Y--;
            
            var bName = string.Format( "Block {0}, {1}"    , block.X, block.Y );
            var sName = string.Format( "Sub-Block {0}, {1}", sub.X  , sub.Y   );
            //DebugLog.WriteLine( bName );
            //DebugLog.WriteLine( sName );
            
            #endregion
            
            var wsHandles = Worldspace.Handles;
            
            // Look through every mod handle for the worldspace
            foreach( var wsHandle in wsHandles )
            {
                ElementHandle[] bHandles = null;
                
                var gHandle = wsHandle.GetChildGroup();
                if( !gHandle.IsValid() )
                    goto localSkipGroup;
                
                bHandles = gHandle.GetElements<ElementHandle>();
                if( bHandles.NullOrEmpty() )
                    goto localSkipGroup;
                
                // Look through every block in the worldspace
                var bCount = bHandles.Length;
                var found = false;
                for( int bIndex = 0; bIndex < bCount; bIndex++ )
                {
                    ElementHandle[] sbHandles = null;
                    
                    var bHandle = bHandles[ bIndex ];
                    if( !bHandle.IsValid() )
                        goto localSkipBlock;
                    
                    var tbName = bHandle.Name;
                    var bMatch = tbName == bName;
                    //DebugLog.WriteLine( string.Format( "Testing \"{0}\" ? \"{1}\" = {2}", tbName, bName, bMatch.ToString() ) );
                    if( !bMatch )
                        goto localSkipBlock;
                    
                    sbHandles = bHandle.GetElements<ElementHandle>();
                    if( sbHandles.NullOrEmpty() )
                        goto localSkipBlock;
                    
                    // Look through every sub-block in the block
                    var sbCount = sbHandles.Length;
                    for( int sbIndex = 0; sbIndex < sbCount; sbIndex++ )
                    {
                        var sbHandle = sbHandles[ sbIndex ];
                        if( !sbHandle.IsValid() )
                            continue;
                        
                        var tsbName = sbHandle.Name;
                        var sbMatch = tsbName == sName;
                        //DebugLog.WriteLine( string.Format( "Testing \"{0}\" ? \"{1}\" = {2}", tsbName, sName, sbMatch.ToString() ) );
                        if( !sbMatch )
                            continue;
                        
                        // Found the sub-block, now load all the cells in this sub-group
                        found = true;
                        //DebugLog.WriteLine( "Loading Cells from " + tbName + " " + tsbName + " in " + wsHandle.Filename );
                        LoadFromEx( ParentForm, sbHandle );
                        break;
                    }
                    
                localSkipBlock:
                    ElementHandle.ReleaseHandles<ElementHandle>( sbHandles );
                    if( found ) break;
                }
                
            localSkipGroup:
                ElementHandle.ReleaseHandles<ElementHandle>( bHandles );
                if( gHandle.IsValid() )
                    gHandle.Dispose();
            }
            
            // Loaded all the cells in the sub-block now (in theory)
            //DebugLog.WriteLine( "Final attempt to find grid in dictionary" );
            if( _ByGrid != null )
                _ByGrid.TryGetValue( coords, out cell );
            
        localReturnResult:
            //DebugLog.CloseIndentLevel<Plugin.Forms.Cell>( cell );
            return cell;
        }
        
        public Plugin.Forms.Cell Persistent { get { return _Persistent; } }
        
        public override bool Add( IXHandle syncObject )
        {
            //DebugLog.OpenIndentLevel( new [] { this.GetType().ToString(), "Add()", syncObject.ToString() } );
            var result = false;
            
            // Do the base addition
            if( !base.Add( syncObject ) )
                goto localReturnResult;
            
            // Not a cell?  wth?
            var cell = syncObject as Plugin.Forms.Cell;
            if( cell == null )
            {
                DebugLog.WriteError( this.GetType().ToString(), "Add()", string.Format( "Invalid object to add to collection.\nCollection = {0}\nObject = {1}", this.ToString(), syncObject.ToString() ) );
                goto localReturnResult;
            }
            
            // Don't store interior cells by grid or persistence
            if( cell.GetIsInterior( Engine.Plugin.TargetHandle.WorkingOrLastFullRequired ) )
            {
                result = true;
                goto localReturnResult;
            }
            
            // Don't store interior cells by grid or persistence
            if( Worldspace == null )
            {
                result = true;
                goto localReturnResult;
            }
            
            // Store the worldspace's unique persistent cell
            if( !cell.RecordFlags.GetPersistent( Engine.Plugin.TargetHandle.WorkingOrLastFullRequired ) )
            {
                _Persistent = cell;
                result = true;
                goto localReturnResult;
            }
            
            // No grid?
            if( !cell.CellGrid.HasValue( TargetHandle.WorkingOrLastFullRequired ) )
                goto localReturnResult;
            
            // Store the worldspaces exterior cell in the grid dictionary
            if( _ByGrid == null )
                _ByGrid = new Dictionary<Vector2i, Engine.Plugin.Forms.Cell>();
            
            _ByGrid[ cell.CellGrid.GetGrid( TargetHandle.WorkingOrLastFullRequired ) ] = cell;
            
            result = true;
            
        localReturnResult:
            //DebugLog.CloseIndentLevel( result.ToString() );
            return result;
        }
        
        public override void Remove( IXHandle syncObject )
        {
            base.Remove( syncObject );
            if( ( syncObject == null )||( _ByGrid == null ) )
                return;
            
            // Not a cell?  wth?
            var cell = syncObject as Plugin.Forms.Cell;
            if( cell == null )
                return;
            
            _ByGrid.Remove( cell.CellGrid.GetGrid( TargetHandle.Working ) );
        }
        
        public Plugin.Forms.Worldspace Worldspace
        {
            get
            {
                return _Ancestor as Plugin.Forms.Worldspace;
            }
        }
        
    }
    
}