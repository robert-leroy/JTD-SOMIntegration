﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace jtd_som_gl.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "14.0.0.0")]
    internal sealed partial class gl : global::System.Configuration.ApplicationSettingsBase {
        
        private static gl defaultInstance = ((gl)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new gl())));
        
        public static gl Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"SELECT 
	(SELECT MAX(voucher_no)+1 from apinv_hdr) as TransactionNumber
	, SUM(d.SHPQTY * d.UNTCST) as Amount
FROM SOM_SomShipmentDetail d
JOIN SOM_SomShipmentHeader h ON h.INVNBR = d.INVNBR AND h.INVSEQ = d.INVSEQ
WHERE h.INVTYP = 'IV'  and d.WHS = 'OHT'")]
        public string SqlQueryGlEntry {
            get {
                return ((string)(this["SqlQueryGlEntry"]));
            }
            set {
                this["SqlQueryGlEntry"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("http://jtdsql02.jtdinc.local:3080/api")]
        public string rootUri {
            get {
                return ((string)(this["rootUri"]));
            }
            set {
                this["rootUri"] = value;
            }
        }
    }
}
