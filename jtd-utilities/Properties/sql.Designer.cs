﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace jtd_utilities.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "14.0.0.0")]
    internal sealed partial class sql : global::System.Configuration.ApplicationSettingsBase {
        
        private static sql defaultInstance = ((sql)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new sql())));
        
        public static sql Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Data Source=jtdsql02.jtdinc.local;Initial Catalog=p21;User ID=admin;Password=6969" +
            "46")]
        public string connectionString {
            get {
                return ((string)(this["connectionString"]));
            }
            set {
                this["connectionString"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("admin")]
        public string userName {
            get {
                return ((string)(this["userName"]));
            }
            set {
                this["userName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("696946")]
        public string passWord {
            get {
                return ((string)(this["passWord"]));
            }
            set {
                this["passWord"] = value;
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
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("bob@leroynet.com")]
        public string MailUsername {
            get {
                return ((string)(this["MailUsername"]));
            }
            set {
                this["MailUsername"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Futur3W@v3")]
        public string MailPassword {
            get {
                return ((string)(this["MailPassword"]));
            }
            set {
                this["MailPassword"] = value;
            }
        }
    }
}
