﻿/*
 * File.cs
 * 
 * Fallout 4 master/plugin file.
 * 
 */

using System;
using System.Collections.Generic;

using XeLib;
using XeLib.API;

using Engine.Plugin.Attributes;
using Engine.Plugin.Interface;


namespace Engine.Plugin
{
    
    public class File : IDisposable, IXHandle
    {
        
        #region Meta
        
        readonly string                 _Filename = null;
        FileHandle                      _Handle = null;
        
        #endregion
        
        /*
        public static string            DebugIDString( File file )
        {
            return file == null ? "null" : file.Filename;
        }
        */
        
        #region Allocation & Disposal
        
        #region Allocation
        
        public                          File( FileHandle handle )
        {
            if( !handle.IsValid() )
                throw new ArgumentNullException( "handle" );
            _Filename = handle.Filename;
            _Handle = handle;
        }
        
        #endregion
        
        #region Disposal
        
        // Handle "double-free" by combinations of explicit disposal[s] and GC disposal
        
        protected bool                  Disposed = false;
        
                                       ~File()
        {
            Dispose( true );
        }
        
        public void                     Dispose()
        {
            Dispose( true );
        }
        
        protected virtual void          Dispose( bool disposing )
        {
            if( Disposed )
                return;
            
            if( _Handle.IsValid() )
                _Handle.Dispose();
            _Handle = null;
            
            Disposed = true;
        }
        
        #endregion
        
        #endregion
        
        public bool                     Save( string filePath = "" )
        {
            return _Handle.IsValid() && _Handle.SaveFile( filePath );
        }
        
        public string                   Filename                    { get { return _Filename; } }
        
        #region IXHandle
        
        public override int             GetHashCode()               { return _Filename.GetHashCode(); }
        
        public override string          ToString()
        {
            if( this == null )
                return "[null]";
            if( Disposed )
                return "[disposed]";
            var strH = _Handle.ToStringNullSafe();
            var str = string.Format(
                "[{0} :: typeof( File ) = {1}{2}]",
                _Filename,
                this.TypeFullName(),
                ( strH == null ? null : string.Format( " :: handle = {0}", strH ) )
            );
            return str;
        }

        public string                   IDString                    { get { return string.Format( "IXHandle.IDString".Translate(), LoadOrder.ToString( "X2" ), Filename ); } }

        #region Required Properties

        public string                   Signature                   { get { return _Handle.Signature; } }
        
        public IXHandle                 Ancestor
        {
            get { return null; }
            set { throw new NotImplementedException(); }
        }
        
        public File[]                   Files                       { get { return new []{ this }; } }
        
        public string[]                 Filenames                   { get { return new []{ _Filename }; } }

        public string                   GetFilename( TargetHandle target )
        {
            return _Filename;
        }

        public uint                     LoadOrder                   { get { return _Handle.LoadOrder; } }
        
        /*
        public uint                     FormID                      { get { return _Handle.LoadOrder << 24; } }
        
        public string                   EditorID                    { get { return _Filename; } }
        */
        
        public uint                     GetFormID( Engine.Plugin.TargetHandle target )
        {
            return _Handle.LoadOrder << 24;
        }
        public void                     SetFormID( Engine.Plugin.TargetHandle target, uint value )
        {
            throw new NotImplementedException();
        }
        
        public string                   GetEditorID( Engine.Plugin.TargetHandle target )
        {
            return _Filename;
        }
        public void                     SetEditorID( Engine.Plugin.TargetHandle target, string value )
        {
            throw new NotImplementedException();
        }
        
        public ConflictStatus           ConflictStatus
        {
            get
            {
                return MasterHandle.IsValid()
                    ? Engine.Plugin.ConflictStatus.NoConflict
                    : Engine.Plugin.ConflictStatus.Invalid;
            }
        }
        
        #endregion
        
        #region Un/Loading
        
        public bool                     Load()                      { return true; }
        
        public bool                     PostLoad()                  { return true; }
        
        // For Dispose() see the Allocation/Deallocation region above
        
        #endregion
        
        #region XeLib Handles
        
        #region Files
        
        public ElementHandle            CopyAsOverride()            { return null; }
        
        public bool                     IsInWorkingFile()           { return this == GodObject.Plugin.Data.Files.Working; }
        
        public bool                     IsInFile( Plugin.File file ) { return file == this; }
        
        public bool                     IsModifiedIn( Plugin.File file )
        {
            return file == this && _Handle.IsModified;
        }
        
        public ElementHandle            HandleFor( Plugin.File file )
        {
            return file == this ? _Handle : null;
        }
        
        #endregion
        
        #region Raw Handles
        
        public bool                     IsHandleFor( ElementHandle handle ) { return handle.IsValid() && handle.DuplicateOf( _Handle ); }
        
        public bool                     AddNewHandle( ElementHandle newHandle ) { return false; }
        
        public ElementHandle            MasterHandle                { get { return _Handle; } }
        public ElementHandle            WorkingFileHandle           { get { return _Handle; } }
        
        public ElementHandle            LastFullRequiredHandle      { get { return _Handle; } }
        public ElementHandle            LastFullOptionalHandle      { get { return _Handle; } }
        public ElementHandle            LastHandleBeforeWorkingFile { get { return _Handle; } }
        
        public List<ElementHandle>      Handles
        {
            get
            {
                var list = new List<ElementHandle>();
                list.Add( _Handle );
                return list;
            }
        }
        
        #endregion
        
        #endregion
        
        #region Parent/Child collection[s]
        
        // Files don't directly handle collections, they are managed as a global pool (GodObject.Plugin.Data.Root.cs)
        
        #region Parent container collection
        
        public Collection               ParentCollection
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
        
        #endregion
        
        #region Child collections
        
        public void                     AddCollection( Collection collection )
        {   throw new NotImplementedException(); }
        
        public Collection               CollectionFor( string signature )
        {   throw new NotImplementedException(); }
        
        public Collection               CollectionFor<TSync>() where TSync : class, IXHandle
        {   throw new NotImplementedException(); }
        
        public Collection               CollectionFor( ClassAssociation association )
        {   throw new NotImplementedException(); }
        
        public List<Collection>         ChildCollections
        { get { throw new NotImplementedException(); } }
        
        #endregion
        
        #endregion
        
        #endregion
        
        #region Type and status
        
        public bool                     IsModified                  { get { return _Handle.IsModified; } }
        
        public bool                     IsESM                       { get { return _Handle.IsESM; } }
        public bool                     IsESL                       { get { return _Handle.IsESL; } }
        public bool                     IsESP                       { get { return _Handle.IsESP; } }
        
        public bool                     BuildReferences()
        {
            return Setup.BuildReferencesEx( _Handle.XHandle, false );
        }

        #endregion

        #region ISyncedListViewObject

        /*

        public event EventHandler  ObjectDataChanged;

        public string ExtraInfo { get { return null; } }

        public void SendObjectDataChangedEvent()
        {
            EventHandler handler = ObjectDataChanged;
            if( handler != null )
                handler( this, null );
        }

        public virtual bool InitialCheckedOrSelectedState()
        {
            return false;
        }

        public virtual bool ObjectChecked( bool checkedValue )
        {
            return checkedValue;
        }

        */

        #endregion

    }
    
}
