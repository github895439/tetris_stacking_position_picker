﻿//------------------------------------------------------------------------------
// <auto-generated>
//     このコードはツールによって生成されました。
//     ランタイム バージョン:4.0.30319.42000
//
//     このファイルへの変更は、以下の状況下で不正な動作の原因になったり、
//     コードが再生成されるときに損失したりします。
// </auto-generated>
//------------------------------------------------------------------------------

namespace tetris_stacking_position_picker.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "14.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0.95")]
        public double GAME_START_AND_END_BLIGHTNESS_MIN {
            get {
                return ((double)(this["GAME_START_AND_END_BLIGHTNESS_MIN"]));
            }
            set {
                this["GAME_START_AND_END_BLIGHTNESS_MIN"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0.1")]
        public double EXIST_BLIGHTNESS_FIELD_MIN {
            get {
                return ((double)(this["EXIST_BLIGHTNESS_FIELD_MIN"]));
            }
            set {
                this["EXIST_BLIGHTNESS_FIELD_MIN"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0.25")]
        public double EXIST_BLIGHTNESS_NEXT_MIN {
            get {
                return ((double)(this["EXIST_BLIGHTNESS_NEXT_MIN"]));
            }
            set {
                this["EXIST_BLIGHTNESS_NEXT_MIN"] = value;
            }
        }
    }
}
