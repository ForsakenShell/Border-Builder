﻿/*
 * ImportTarget.cs
 *
 * Base import target class.
 *
 */
using System;

using Engine.Plugin;
using Engine.Plugin.Interface;
using Engine.Plugin.Attributes;
using Engine.Plugin.Extensions;


namespace GUIBuilder.FormImport
{
    
    public static partial class Extensions
    {
        
        public static string NullSafeIDString( this ImportTarget target, string format = null, string unresolveableSuffix = null )
        {
            if( string.IsNullOrEmpty( format ) ) format = "{0}";
            if( target == null ) return string.Format( format, "[null]" );
            //return ImportBase.ExtraInfoFor( f, Value, FormID, EditorID, unresolveable, extra );
            var tmp = !target.Resolveable()
                ? string.IsNullOrEmpty( unresolveableSuffix )
                    ? string.Format(
                        "IXHandle.IDString".Translate(),
                        target.FormID.ToString( "X8" ),
                        target.EditorID )
                    : string.Format(
                        "{0} {1}",
                        string.Format(
                            "IXHandle.IDString".Translate(),
                            target.FormID.ToString( "X8" ),
                            target.EditorID
                        ),
                        unresolveableSuffix )
                : string.Format(
                    "IXHandle.IDString".Translate(),
                    target.CurrentFormID( Engine.Plugin.TargetHandle.Master ).ToString( "X8" ),
                    target.CurrentEditorID( Engine.Plugin.TargetHandle.WorkingOrLastFullRequired ) );
            return string.Format( format, tmp, target.DisplayName );
        }

    }

    public class ImportTarget
    {

        ImportBase                                      _Parent = null;
        public ImportBase                               Parent
        {
            get
            {
                return _Parent;
            }
        }

        IXHandle                                        _Value = null;
        public IXHandle                                 Value
        {
            get { return _Value; }
            set
            {
                _Value = value;
                SyncImportReferenceInfoWithResolvedTarget();
            }
        }
        public bool                                     IsResolved
        {
            get
            {
                return _Value != null;
            }
        }


        string                                          _DisplayName = null;
        public string                                   DisplayName
        {
            get
            {
                return _DisplayName;
            }
        }

        Type                                            _Type = null;
        public Type                                     Type
        {
            get
            {
                return _Type;
            }
        }

        ClassAssociation                                _Association = null;
        public ClassAssociation                         Association
        {
            get
            {
                return _Association;
            }
        }

        uint                                            _FormID = Engine.Plugin.Constant.FormID_Invalid;
        public uint                                     FormID
        {
            get
            {
                return _FormID;
            }
        }

        string                                          _EditorID = null;
        public string                                   EditorID
        {
            get
            {
                return _EditorID;
            }
        }
        internal void                                   SetEditorID( string value )
        {
            _EditorID = value;
        }
        
        public uint                                     CurrentFormID( TargetHandle target )
        {
            return
                _Value == null
                ? _FormID
                : _Value.GetFormID( target );
        }
        
        public string                                   CurrentEditorID( TargetHandle target )
        {
            return
                _Value == null
                ? _EditorID
                : _Value.GetEditorID( target );
        }

        #region Constructors

        public                                          ImportTarget(
            ImportBase parent,
            string displayName,
            Type type,
            string editorID )
        {
            if( string.IsNullOrEmpty( editorID ) )
                throw new NullReferenceException( "'editorID' cannot be null" );
            INTERNAL_Constructor( parent, displayName, type, editorID, null );
        }

        public                                          ImportTarget(
            ImportBase parent,
            string displayName,
            Type type,
            IXHandle target,
            string editorID = null )
        {
            if( ( string.IsNullOrEmpty( editorID ) )&&( target == null ) )
                throw new NullReferenceException( "'target' cannot be null" );
            INTERNAL_Constructor( parent, displayName, type, editorID ?? target.GetEditorID( TargetHandle.WorkingOrLastFullRequired ), target );
        }

        private void                                    INTERNAL_Constructor(
            ImportBase parent,
            string displayName,
            Type type,
            string editorID,
            IXHandle target )
        {
            if( type == null )
                throw new NullReferenceException( "'type' cannot be null" );
            if( !type.HasInterface<IXHandle>() )
                throw new NullReferenceException( "'type' must implement Engine.Plugin.Interface.IXHandle" );
            
            _Parent         = parent;
            _DisplayName    = displayName;
            _FormID         = target == null
                            ? Engine.Plugin.Constant.FormID_Invalid
                            : target.GetFormID( TargetHandle.Master );
            _EditorID       = editorID;
            _Value          = target;
            _Type           = type;
            _Association    = Reflection.AssociationFrom( _Type );
        }

        #endregion

        public bool                                     Matches<T>( T other, bool allowClearing ) where T : class, IXHandle
        {
            if(
                ( !allowClearing )&&
                (
                    ( other == null )||
                    ( !Resolveable() )
                )
            )   return false;
            var otherFormID     = ( other != null ) ? other.GetFormID( TargetHandle.Master ) : Engine.Plugin.Constant.FormID_Invalid;
            var otherEditorID   = other?.GetEditorID( TargetHandle.WorkingOrLastFullRequired );
            
            var fm = Matches( otherFormID, allowClearing );
            var em = Matches( otherEditorID );
            var fe = FormID == Engine.Plugin.Constant.FormID_Invalid;
            var ee = string.IsNullOrEmpty( EditorID );
            
            var result = allowClearing && fe && ee;
            if( !fe &&  ee ) result = fm;
            if(  fe && !ee ) result = em;
            if( !fe && !ee ) result = fm && em;
            
            //DebugLog.WriteLine( new [] { this.FullTypeName(), "Matches<T>()", "allowClearing = " + allowClearing.ToString(), "FormID ? " + fm.ToString(), "EditorID ? " + em.ToString(), "result = " + result.ToString() } );
            return result;
        }
        
        public bool                                     Matches( uint otherFormID, bool allowClearing )
        {
            return (
                (
                    ( allowClearing )||
                    (
                        ( FormID != Engine.Plugin.Constant.FormID_Invalid ) &&
                        ( otherFormID != Engine.Plugin.Constant.FormID_Invalid )
                    )
                )&&
                ( Engine.Plugin.Constant.ValidFormID( FormID      ) )&&
                ( Engine.Plugin.Constant.ValidFormID( otherFormID ) )&&
                ( FormID == otherFormID )
            );
        }
        
        public bool                                     Matches( string otherEditorID )
        {
            return (
                ( Engine.Plugin.Constant.ValidEditorID( EditorID      ) )&&
                ( Engine.Plugin.Constant.ValidEditorID( otherEditorID ) )&&
                ( EditorID.InsensitiveInvariantMatch( otherEditorID ) )
            );
        }
        
        public bool                                     Resolveable()
        {
            return
                ( _Value != null )||
                (
                    ( Association != null )&&
                    (
                        ( Engine.Plugin.Constant.ValidFormID  ( FormID   ) )||
                        ( Engine.Plugin.Constant.ValidEditorID( EditorID ) )
                    )
                );
        }
        
        public virtual bool                             Resolve( bool errorIfUnresolveable = true )
        {
            if( _Value == null )
            {
                var vFID = _FormID  .ValidFormID  ();
                var vEID = _EditorID.ValidEditorID();

                #region Try resolve as Form target
                var testType = typeof( Form );
                if( _Type.IsClassOrSubClassOf( testType ) )
                {
                    if( vFID )
                        Value = GodObject.Plugin.Data.Root.Find( _Association, _FormID  , true );

                    else if( vEID )
                        Value = GodObject.Plugin.Data.Root.Find( _Association, _EditorID, true );
                }
                #endregion
                
                #region Try resolve as PapyrusScript target
                testType = typeof( PapyrusScript );
                if( _Type.IsClassOrSubClassOf( testType ) )
                {
                    if( vFID )
                        Value = GodObject.Plugin.Data.GetScriptByFormID( _FormID );
                    else if( vEID )
                        Value = GodObject.Plugin.Data.GetScriptByEditorID( _EditorID );
                }
                #endregion

            }
            if( ( errorIfUnresolveable )&&( _Value == null ) )
                Parent.AddErrorMessage( FormImport.ErrorTypes.Resolve, this.NullSafeIDString( "Cannot resolve {0} for {1}" ) );
            return ( _Value != null );
        }

        public virtual bool                             CreateNewFormInWorkingFile()
        {
            try
            {
                var container = GodObject.Plugin.Data.Root.GetCollection( Association, true, false, false );
                if( container == null )
                {
                    Parent.AddErrorMessage( ErrorTypes.Import, "Unable to get root container for " + Type.FullName() );
                    return false;
                }
                var handle = container.CreateNew();
                if( handle == null )
                {
                    Parent.AddErrorMessage( ErrorTypes.Import, string.Format(
                        "Unable to create a new {0} in \"{1}\"",
                        Type.FullName(),
                        GodObject.Plugin.Data.Files.Working.Filename ) );
                    return false;
                }
                Value = handle;
                return true;
            }
            catch( Exception e )
            {
                Parent.AddErrorMessage( ErrorTypes.Import, string.Format(
                    "An exception occured when trying to create a new {0} in \"{1}\"!\nInner exception:\n{2}",
                    Type.FullName(), 
                    GodObject.Plugin.Data.Files.Working.Filename,
                    e.ToString() ) );
            }
            return false;
        }

        public virtual bool                             CopyToWorkingFile()
        {
            if( _Value == null )
            {
                Parent.AddErrorMessage( ErrorTypes.Import, string.Format( "{0} is unresolved", DisplayName ) );
                return false;
            }

            string errorMessage = null;
            try
            {
                if( _Value.IsInWorkingFile() )
                    return true;
                if( _Value.CopyAsOverride() != null )
                    return true;
                errorMessage = string.Format(
                    "Unable to copy override for {0}",
                    _Value.ExtraInfoFor( _Value.GetFormID( TargetHandle.Master ), _Value.GetEditorID( TargetHandle.WorkingOrLastFullRequired ), unresolveable: "unresolved" )
                );
            }
            catch( Exception e )
            {
                errorMessage = string.Format(
                    "An exception occured when trying to copy override for {0}\nInner Exception:\n{1}",
                    _Value.ExtraInfoFor( _Value.GetFormID( TargetHandle.Master ), _Value.GetEditorID( TargetHandle.WorkingOrLastFullRequired ), unresolveable: "unresolved" ),
                    e.ToString() );
            }

            Parent.AddErrorMessage( ErrorTypes.Import, errorMessage );
            return false;
        }

        protected void                                  SyncImportReferenceInfoWithResolvedTarget()
        {
            if( _Value != null )
            {
                _FormID = Value.GetFormID( Engine.Plugin.TargetHandle.Master );
                _EditorID = Value.GetEditorID( Engine.Plugin.TargetHandle.WorkingOrLastFullRequired );
            }
        }
        
    }
}
