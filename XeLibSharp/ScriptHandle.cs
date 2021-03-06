using System;
using XeLib.API;
using XeLib.Internal;

namespace XeLib
{
    
    [HandleMapping( new[] { Elements.ElementTypes.EtStruct } )]
    public class ScriptHandle : ElementHandle
    {
        
        public ScriptHandle( uint uHandle ) : base( uHandle ) {}
        
        public override string ToStringExtra()
        {
            return Disposed
                ? null
                : string.Format( "Script = \"{0}\"", this.ScriptName );
        }
        
        #region Record ID - ScriptName
        
        public string ScriptName
        {
            get
            {
                return ElementValues.GetValueEx( XHandle, "scriptName" );
            }
            set
            {
                ElementValues.SetValueEx( XHandle, "scriptName", value );
            }
        }
        
        #endregion
        
        public override string Signature { get { return null; } }
        
        public ScriptPropertyHandle GetProperty( string propertyName )
        {
            var propertiesElements = GetElements<ScriptPropertyHandle>( "Properties" );
            if( propertiesElements.NullOrEmpty() ) return null;
            ScriptPropertyHandle result = null;
            foreach( var propertyElement in propertiesElements )
            {
                //propertyElement.DebugDumpChildElements( true );
                var nameElement = propertyElement.GetElement<XeLib.ElementHandle>( "propertyName" );
                if( nameElement.IsValid() )
                {
                    var elementValue = nameElement.GetValue();
                    if( elementValue.InsensitiveInvariantMatch( propertyName ) )
                        result = propertyElement;
                    nameElement.Dispose();
                }
                if( ( result == null )||( result.XHandle != propertyElement.XHandle ) )
                    propertyElement.Dispose();
            }
            return result;
        }
        
        public ScriptPropertyHandle AddProperty( ScriptPropertyHandle.PropertyTypes type, string propertyName )
        {
            var result = GetProperty( propertyName );
            if( !result.IsValid() )
            {
                result = Elements.AddArrayItemEx<ScriptPropertyHandle>( XHandle, "Properties", "propertyName", propertyName );
                if( result.IsValid() )
                {
                    ElementValues.SetIntValueEx( result.XHandle, "Type", (int)type );
                    ElementValues.SetIntValueEx( result.XHandle, "Flags", 1 );
                }
            }
            return result;
        }
        
        public bool RemoveProperty( string propertyName )
        {
            var property = GetProperty( propertyName );
            return !property.IsValid() || Elements.RemoveArrayItemEx( XHandle, "Properties", "propertyName", property.PropertyName );
        }
        
    }
}