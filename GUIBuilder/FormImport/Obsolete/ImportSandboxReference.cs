﻿/*
 * ImportSandboxReference.cs
 *
 * Sub-division/Workshop sandbox reference (REFR(ACTI)).
 *
 * OBSOLETE - Use ImportBase
 */
 /*
using System;
using System.Collections.Generic;
using System.Linq;
using Engine.Plugin.Extensions;
using Maths;
using AnnexTheCommonwealth;


namespace GUIBuilder.FormImport
{
    
    public class ImportSandboxReference : ImportBase
    {
        const string            IMPORT_SIGNATURE = "SandboxRef";

        public const uint       FO4_TARGET_RECORD_FLAGS = 0;

        public const uint       ATC_TARGET_RECORD_FLAGS =
            (uint)Engine.Plugin.Forms.Fields.Record.Flags.Common.Persistent |
            (uint)Engine.Plugin.Forms.Fields.Record.Flags.REFR.InitiallyDisabled |
            (uint)Engine.Plugin.Forms.Fields.Record.Flags.REFR.NoRespawn;
        
        string                  NewEditorID = null;
        FormTarget              ftBaseForm = null;
        FormTarget              ftWorldspace = null;
        FormTarget              ftCell = null;
        Vector3f                Position = Vector3f.MinValue;
        Vector3f                Rotation = Vector3f.Zero;
        Vector3f                Bounds = Vector3f.Zero;
        FormTarget              ftLinkRef = null;
        FormTarget              ftLinkKeyword = null;
        FormTarget              ftLayer = null;
        System.Drawing.Color    Color = System.Drawing.Color.Empty;
        
        //Engine.Plugin.Forms.ObjectReference TargetRef           { get { return Target         == null ? null : Target.Form           as Engine.Plugin.Forms.ObjectReference; } }
        Engine.Plugin.Form      TargetBaseForm                  { get { return ftBaseForm     == null ? null : ftBaseForm.Form; } }
        Engine.Plugin.Forms.Worldspace TargetWorldspace         { get { return ftWorldspace   == null ? null : ftWorldspace.Form     as Engine.Plugin.Forms.Worldspace; } }
        Engine.Plugin.Forms.Cell TargetCell                     { get { return ftCell         == null ? null : ftCell.Form           as Engine.Plugin.Forms.Cell; } }
        Engine.Plugin.Forms.ObjectReference TargetLinkRef       { get { return ftLinkRef      == null ? null : ftLinkRef.Form        as Engine.Plugin.Forms.ObjectReference; } }
        Engine.Plugin.Forms.Keyword TargetLinkKeyword           { get { return ftLinkKeyword  == null ? null : ftLinkKeyword.Form    as Engine.Plugin.Forms.Keyword; } }
        Engine.Plugin.Forms.Layer TargetLayer                   { get { return ftLayer        == null ? null : ftLayer.Form          as Engine.Plugin.Forms.Layer; } }
        
        protected override void         DumpImport()
        {
            //return;
            base.DumpImport();
            DebugLog.WriteStrings( null, new[]{
                string.IsNullOrEmpty( NewEditorID ) ? null : string.Format( "NewEditorID = \"{0}\"", NewEditorID ),
                ftBaseForm      .NullSafeIDString( "BaseForm = {0}" ),
                ftWorldspace    .NullSafeIDString( "Worldspace = {0}" ),
                ftCell          .NullSafeIDString( "Cell = {0}" ),
                string          .Format       ( "Position = {0}", Position.ToString() ),
                string          .Format       ( "Rotation = {0}", Rotation.ToString() ),
                string          .Format       ( "Bounds = {0}", Bounds.ToString() ),
                string          .Format       ( "Color = {0}", Color.ToString() ),
                ftLinkRef       .NullSafeIDString( "LinkRef = {0}" ),
                ftLinkKeyword   .NullSafeIDString( "LinkKeyword = {0}" ),
                ftLayer         .NullSafeIDString( "Layer = {0}" ) },
                false, true, false, false );
        }
        
        public                          ImportSandboxReference( Engine.Plugin.Forms.ObjectReference originalVolume, string newEditorID, Engine.Plugin.Form volumeBase, Engine.Plugin.Forms.Worldspace worldspace, Engine.Plugin.Forms.Cell cell, Vector3f position, Vector3f rotation, Vector3f bounds, System.Drawing.Color color, Engine.Plugin.Forms.ObjectReference linkRef, Engine.Plugin.Forms.Keyword linkKeyword, Engine.Plugin.Forms.Layer layer, string layerEditorID, uint recordFlags )
            : base( IMPORT_SIGNATURE, recordFlags, false, typeof( Engine.Plugin.Forms.ObjectReference ), originalVolume )
        {
            if( string.IsNullOrEmpty( newEditorID ) )
                throw new Exception( string.Format( "{0} :: cTor() :: newEditorID cannot be null!", this.TypeFullName() ) );
            
            if( !Target.IsResolved )
                Target.EditorID = newEditorID;
            NewEditorID     = newEditorID;
            ftBaseForm      = new FormTarget( "Base Form", this, typeof( Engine.Plugin.Form ), volumeBase );
            ftWorldspace    = new FormTarget( "Worldspace", this, typeof( Engine.Plugin.Forms.Worldspace ), worldspace );
            ftCell          = new FormTarget( "Cell", this, typeof( Engine.Plugin.Forms.Cell ), cell );
            Position        = new Vector3f( position );
            Rotation        = new Vector3f( rotation );
            Bounds          = new Vector3f( bounds );
            Color           = System.Drawing.Color.FromArgb( color.A, color.R, color.G, color.B );
            ftLinkRef       = new FormTarget( "Linked Ref", this, typeof( Engine.Plugin.Forms.ObjectReference ), linkRef );
            ftLinkKeyword   = new FormTarget( "Linked Ref Keyword", this, typeof( Engine.Plugin.Forms.Keyword ), linkKeyword );
            if( layer != null )
                ftLayer     = new FormTarget( "Layer", this, typeof( Engine.Plugin.Forms.Layer ), layer );
            else if( !string.IsNullOrEmpty( layerEditorID ) )
                ftLayer     = new FormTarget( "Layer", this, typeof( Engine.Plugin.Forms.Layer ), Engine.Plugin.Constant.FormID_None, layerEditorID );
            else
                ftLayer     = null;
            DumpImport();
        }
        
        public                          ImportSandboxReference( string[] importData, uint recordFlags )
            : base( IMPORT_SIGNATURE, recordFlags, false, typeof( Engine.Plugin.Forms.ObjectReference ), importData )
        {
            ftBaseForm      = new FormTarget( "Base Form", this, typeof( Engine.Plugin.Form ) );
            ftWorldspace    = new FormTarget( "Worldspace", this, typeof( Engine.Plugin.Forms.Worldspace ) );
            ftCell          = new FormTarget( "Cell", this, typeof( Engine.Plugin.Forms.Cell ) );
            ftLinkRef       = new FormTarget( "Linked Ref", this, typeof( Engine.Plugin.Forms.ObjectReference ) );
            ftLinkKeyword   = new FormTarget( "Linked Ref Keyword", this, typeof( Engine.Plugin.Forms.Keyword ) );
            ftLayer         = new FormTarget( "Layer", this, typeof( Engine.Plugin.Forms.Layer ) );
            DumpImport();
        }
        
        Engine.Plugin.Forms.ObjectReference OriginalLinkParent( Engine.Plugin.Forms.Keyword linkKeyword )
        {
            var refr = TargetRef;
            if( ( refr == null )||( linkKeyword == null ) )
                return null;
            
            var refrRefs = refr.References;
            if( refrRefs.NullOrEmpty() ) return null;
            
            var keywordFormID = linkKeyword.GetFormID( Engine.Plugin.TargetHandle.Master );
            
            foreach( var form in refrRefs )
            {
                var refrRef = form as Engine.Plugin.Forms.ObjectReference;
                if( refrRef == null ) continue;
                
                var linked = refrRef.LinkedRefs;
                var linkCount = linked.GetCount( Engine.Plugin.TargetHandle.WorkingOrLastFullRequired );
                if( linkCount < 1 ) continue;
                for( int i = 0; i < linkCount; i++ )
                    if( linked.GetKeywordFormID( Engine.Plugin.TargetHandle.WorkingOrLastFullRequired, i ) == keywordFormID )
                        return refrRef;
            }
            
            return null;
        }
        
        protected override string       GetDisplayUpdateFormInfo()
        {
            var tmp = new List<string>();
            var refr = TargetRef;
            
            if( refr.GetNameFormID( Engine.Plugin.TargetHandle.WorkingOrLastFullRequired ) != ftBaseForm.FormID )
                tmp.Add( ftBaseForm.NullSafeIDString( "Base form {0}" ) );
            
            var oldEDID = refr.GetEditorID( Engine.Plugin.TargetHandle.WorkingOrLastFullRequired );
            if( string.Compare( oldEDID, NewEditorID, StringComparison.InvariantCulture ) != 0 )
                tmp.Add( string.Format( "EditorID \"{0}\"", NewEditorID ) );
            
            if( refr.GetPosition( Engine.Plugin.TargetHandle.WorkingOrLastFullRequired ) != Position )
                tmp.Add( string.Format( "Position {0}", Position.ToString() ) );
            
            if( refr.GetRotation( Engine.Plugin.TargetHandle.WorkingOrLastFullRequired ) != Rotation )
                tmp.Add( string.Format( "Rotation {0}", Rotation.ToString() ) );
            
            if( !ftCell.Matches( refr.Cell, false ) )
                tmp.Add( ftCell.NullSafeIDString( "Cell {0}", "unresolved" ) );
            //var cell = TargetCell;
            //if( ( cell != null )&&( refr.Cell != cell ) )
            //    tmp.Add( GenIDataSync.ExtraInfoFor( cell, format: "Cell {0}", unresolveable: "unresolved" ) );
            if( !ftWorldspace.Matches( refr.Worldspace, false ) )
                tmp.Add( ftWorldspace.NullSafeIDString( "Worldspace {0}", "unresolved" ) );
            
            if( refr.Primitive.GetBounds( Engine.Plugin.TargetHandle.WorkingOrLastFullRequired ) != Bounds )
                tmp.Add( string.Format( "Bounds {0}", Bounds.ToString() ) );

            if( refr.Primitive.GetColor( Engine.Plugin.TargetHandle.WorkingOrLastFullRequired ) != Color )
                tmp.Add( string.Format( "Color {0}", Color.ToString() ) );

            if( !ftLayer.Matches( refr.GetLayerFormID( Engine.Plugin.TargetHandle.WorkingOrLastFullRequired ), true ) )
                tmp.Add( ftLayer.NullSafeIDString( "Layer {0}" ) );
            
            if(
                ( ftLinkRef.Resolveable() )&&
                ( ftLinkKeyword.Resolveable() )
            )   {
                var lr = OriginalLinkParent( TargetLinkKeyword );
                if( !ftLinkRef.Matches( lr, false ) )
                {
                    if( lr != null )
                        tmp.Add(
                            lr.ExtraInfoFor(format: "Unlink sandbox from {0}"));
                    tmp.Add(
                        string.Format(
                            "Link {0} to sandbox using keyword {1}",
                            ftLinkRef.NullSafeIDString(),
                            ftLinkKeyword.NullSafeIDString()
                    ) );
                }
            }
            
            if( refr.LocationReference.GetValue( Engine.Plugin.TargetHandle.WorkingOrLastFullRequired ) != Engine.Plugin.Constant.FormID_None )
                tmp.Add( "Clear Location Reference" );
            
            return tmp.ConcatDisplayInfo();
        }
        
        protected override string       GetDisplayNewFormInfo()
        {
            var tmp = new List<string>();
            
            tmp.Add( ftBaseForm.NullSafeIDString( "Placed instance of {0}" ) );
            
            tmp.Add( string.Format( "EditorID \"{0}\"", NewEditorID ) );
            
            tmp.Add( string.Format( "Position {0}", Position.ToString() ) );
            tmp.Add( string.Format( "Rotation {0}", Rotation.ToString() ) );
            //tmp.Add( GenIDataSync.ExtraInfoFor( TargetCell, format: "Cell {0}", unresolveable: "unresolved" ) );
            tmp.Add( ftCell.NullSafeIDString( "Cell {0}", "unresolved" ) );
            tmp.Add( ftWorldspace.NullSafeIDString( "Worldspace {0}", "unresolved" ) );
            
            tmp.Add( string.Format( "Bounds {0}", Bounds.ToString() ) );
            
            tmp.Add( string.Format( "Color {0}", Color.ToString() ) );

            tmp.Add( ftLayer.NullSafeIDString( "Layer {0}" ) );
            
            if(
                ( ftLinkRef.Resolveable() )&&
                ( ftLinkKeyword.Resolveable() )
            )   tmp.Add(
                    string.Format(
                        "Link {0} to sandbox using keyword {1}",
                        ftLinkRef.NullSafeIDString(),
                        ftLinkKeyword.NullSafeIDString()
                ) );
            
            return tmp.ConcatDisplayInfo();
        }
        
        protected override string       GetDisplayEditorID( Engine.Plugin.TargetHandle target )
        {
            var tEDID = TargetRef == null ? null : TargetRef.GetEditorID( target );
            return string.IsNullOrEmpty( tEDID )
                ? NewEditorID
                : tEDID;
        }
        
        public override int             InjectPriority { get { return 50000; } }
        
        protected override bool         ImportDataMatchesTarget()
        {
            if( !Resolve( false ) ) return false;
            var refr = TargetRef;
            if( refr == null )
                return false;
            
            var lwk = ftLinkRef.Resolveable() && ftLinkKeyword.Resolveable();
            if( !lwk ) return false;
            
            var lr = OriginalLinkParent( TargetLinkKeyword );
            
            return
                ( string.Compare( refr.GetEditorID( Engine.Plugin.TargetHandle.WorkingOrLastFullRequired ), NewEditorID, StringComparison.InvariantCulture ) == 0 )&&
                ( ftBaseForm.Matches( refr.GetNameFormID( Engine.Plugin.TargetHandle.WorkingOrLastFullRequired ), false ) )&&
                ( ftWorldspace.Matches( refr.Worldspace, false ) )&&
                ( ftCell.Matches( refr.Cell, false ) )&&
                //( TargetCell == refr.Cell )&&
                ( refr.GetPosition( Engine.Plugin.TargetHandle.WorkingOrLastFullRequired ) == Position )&&
                ( refr.GetRotation( Engine.Plugin.TargetHandle.WorkingOrLastFullRequired ) == Rotation )&&
                ( refr.Primitive.GetBounds( Engine.Plugin.TargetHandle.WorkingOrLastFullRequired ) == Bounds )&&
                ( refr.Primitive.GetColor( Engine.Plugin.TargetHandle.WorkingOrLastFullRequired ) == Color )&&
                ( refr.LocationReference.GetValue( Engine.Plugin.TargetHandle.WorkingOrLastFullRequired ) == Engine.Plugin.Constant.FormID_None )&&
                ( ftLayer.Matches( refr.GetLayerFormID( Engine.Plugin.TargetHandle.WorkingOrLastFullRequired ), true ) )&&
                ( ftLinkRef.Matches( lr, false ) );
        }
        
        protected override bool         ResolveReferenceForms( bool errorIfUnresolveable )
        {
            // Resolve required forms
            ftBaseForm    .Resolve( errorIfUnresolveable );
            ftWorldspace  .Resolve( errorIfUnresolveable );
            ftCell        .Resolve( errorIfUnresolveable );
            if( ( TargetCell == null )&&( TargetWorldspace != null ) )
                ftCell.Form = Engine.Plugin.Forms.Worldspace.GetCellForRefr( TargetWorldspace, Position, RecordFlags );
            //if( ( TargetCell == null )&&( errorIfUnresolveable ) )
            //    AddErrorMessage( ErrorTypes.Resolve, "Unable to resolve target cell in worldspace" );
            
            ftLinkRef     .Resolve( errorIfUnresolveable );
            ftLinkKeyword .Resolve( errorIfUnresolveable );
            
            // Resolve optional forms
            ftLayer       .Resolve( false );
            
            // Minimum forms resolved?
            var minFormsFound =
                ( ftBaseForm.IsResolved )&&
                ( ftWorldspace.IsResolved )&&
                ( TargetCell != null )&&
                (
                    ( ftLinkRef.IsResolved )&&
                    ( ftLinkKeyword.IsResolved )
                );
            
            return minFormsFound;
        }
        
        public override bool            ParseKeyValue( string key, string value )
        {
            switch( key )
            {
                case "ACTIFormID":
                    ftBaseForm.FormID = uint.Parse( value, System.Globalization.NumberStyles.HexNumber );
                    break;
                case "ACTIEDID":
                    ftBaseForm.EditorID = value;
                    break;
                    
                case "WorldspaceFormID":
                    ftWorldspace.FormID = uint.Parse( value, System.Globalization.NumberStyles.HexNumber );
                    break;
                case "WorldspaceEDID":
                    ftWorldspace.EditorID = value;
                    break;
                    
                case "CellFormID":
                    ftCell.FormID = uint.Parse( value, System.Globalization.NumberStyles.HexNumber );
                    break;
                case "CellEDID":
                    ftCell.EditorID = value;
                    break;
                    
                case "Position":
                    Maths.Vector3f.TryParse( value, out Position );
                    break;
                    
                case "Rotation":
                    Maths.Vector3f.TryParse( value, out Rotation );
                    break;
                    
                case "Bounds":
                    Maths.Vector3f.TryParse( value, out Bounds );
                    break;

                case "LinkRefFormID":
                    ftLinkRef.FormID = uint.Parse( value, System.Globalization.NumberStyles.HexNumber );
                    break;
                case "LinkRefEDID":
                    ftLinkRef.EditorID = value;
                    break;
                    
                case "LinkKeywordFormID":
                    ftLinkKeyword.FormID = uint.Parse( value, System.Globalization.NumberStyles.HexNumber );
                    break;
                case "LinkKeywordEDID":
                    ftLinkKeyword.EditorID = value;
                    break;
                    
                case "LayerFormID":
                    ftLayer.FormID = uint.Parse( value, System.Globalization.NumberStyles.HexNumber );
                    break;
                case "LayerEDID":
                    ftLayer.EditorID = value;
                    break;
                    
                default:
                    return false;
            }
            return true;
        }
        
        protected override bool         CreateNewFormInWorkingFile()
        {
            var cell = TargetCell;
            if( cell == null )
            {
                AddErrorMessage( ErrorTypes.Import, "Target Cell is unresolved" );
                return false;
            }
            try
            {
                var refr = cell.ObjectReferences.CreateNew<Engine.Plugin.Forms.ObjectReference>();
                if( refr == null )
                {
                    AddErrorMessage( ErrorTypes.Import, string.Format(
                        "Unable to create a new ObjectReference instance of {0} in cell {1}",
                        ftBaseForm.NullSafeIDString(unresolveableSuffix: "unresolved"),
                        cell.ExtraInfoFor(unresolveable: "unresolved")) );
                    return false;
                }
                refr.SetNameFormID( Engine.Plugin.TargetHandle.WorkingOrLastFullRequired, ftBaseForm.FormID );
                var newVolume = new Volume( refr );
                if( newVolume == null )
                {
                    AddErrorMessage( ErrorTypes.Import, string.Format(
                        "Unable to create a new Script Object on new instance of {0} in cell {1}",
                        GodObject.CoreForms.AnnexTheCommonwealth.Activator.ESM_ATC_ACTI_BorderEnabler.ToString(),
                        ftCell.NullSafeIDString( unresolveableSuffix: "unresolved" ) ) );
                    return false;
                }
                newVolume.PostLoad();
                GodObject.Plugin.Data.SandboxVolumes.Add( newVolume );
                SetTarget( newVolume );
                //TargetForm = refr;
                return true;
            }
            catch( Exception e )
            {
                AddErrorMessage( ErrorTypes.Import, string.Format(
                    "An exception occured when trying to create a new ObjectReference instance of {0} in cell {1}\nInner Exception:\n{2}",
                    ftBaseForm.NullSafeIDString(unresolveableSuffix: "unresolved"),
                    cell.ExtraInfoFor(unresolveable: "unresolved"),
                    e.ToString()) );
            }
            return false;
        }
        
        protected override bool         ApplyImport()
        {
            var refr = Target.Value as Engine.Plugin.Forms.ObjectReference;
            
            refr.SetEditorID( Engine.Plugin.TargetHandle.Working, NewEditorID );
            refr.SetNameFormID( Engine.Plugin.TargetHandle.Working, ftBaseForm.FormID );
            refr.SetPosition( Engine.Plugin.TargetHandle.Working, Position );
            refr.SetRotation( Engine.Plugin.TargetHandle.Working, Rotation );
            refr.Primitive.SetBounds( Engine.Plugin.TargetHandle.Working, Bounds );
            refr.Primitive.SetColor( Engine.Plugin.TargetHandle.Working, Color );
            refr.Primitive.SetUnknown( Engine.Plugin.TargetHandle.Working, 0.3f );
            refr.Primitive.SetType( Engine.Plugin.TargetHandle.Working, Engine.Plugin.Forms.Fields.ObjectReference.Primitive.PrimitiveType.Box );
            
            var newParent = TargetLinkRef;
            var orgParent = OriginalLinkParent( TargetLinkKeyword );
            if( newParent != orgParent )
            {
                if( orgParent != null )
                {
                    orgParent.LinkedRefs.Remove( Engine.Plugin.TargetHandle.Working, refr.GetFormID( Engine.Plugin.TargetHandle.Master ) );
                    orgParent.SendObjectDataChangedEvent( this );
                }
                newParent.LinkedRefs.SetLinkedRef( Engine.Plugin.TargetHandle.WorkingOrLastFullRequired, refr.GetFormID( Engine.Plugin.TargetHandle.Master ), ftLinkKeyword.FormID );
                newParent.SendObjectDataChangedEvent( this );
            }
            
            if( ftLayer.IsResolved )
                refr.SetLayerFormID( Engine.Plugin.TargetHandle.Working, ftLayer.FormID );
            
            // Remove unwanted elements automagically added by the CK/XeLib
            refr.LocationReference.DeleteRootElement( false, false );
            
            return true;
        }
        
    }
    
}
*/