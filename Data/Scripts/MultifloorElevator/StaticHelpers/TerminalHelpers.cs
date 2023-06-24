using System;
using System.Collections.Generic;
using System.Text;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;

namespace Vicizlat.MultifloorElevator
{
    public static class TerminalHelpers
    {
        public static IMyTerminalControlSeparator AddSeparator<T>(Func<IMyTerminalBlock, bool> visible) where T : IMyTerminalBlock
        {
            IMyTerminalControlSeparator sep = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSeparator, T>(string.Empty);
            sep.Visible = visible;
            MyAPIGateway.TerminalControls.AddControl<T>(sep);
            return sep;
        }

        public static IMyTerminalControlLabel AddLabel<T>(string name, string label, Func<IMyTerminalBlock, bool> visible) where T : IMyTerminalBlock
        {
            IMyTerminalControlLabel lab = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, T>(name);
            lab.Label = MyStringId.GetOrCompute(label);
            lab.Visible = visible;
            MyAPIGateway.TerminalControls.AddControl<T>(lab);
            return lab;
        }

        public static IMyTerminalControlCheckbox AddCheckbox<T>(string name, string title, Func<IMyTerminalBlock, bool> getter, Action<IMyTerminalBlock, bool> setter, Func<IMyTerminalBlock, bool> visible, string tooltip = null) where T : IMyTerminalBlock
        {
            IMyTerminalControlCheckbox ch = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCheckbox, T>(name);
            ch.Title = MyStringId.GetOrCompute(title);
            ch.Tooltip = MyStringId.GetOrCompute(tooltip ?? string.Empty);
            ch.Visible = visible;
            ch.Getter = getter;
            ch.Setter = setter;
            MyAPIGateway.TerminalControls.AddControl<T>(ch);
            //CreateProperty(ch, block);
            return ch;
        }

        public static IMyTerminalControlOnOffSwitch AddOnOff<T>(string name, string title, Func<IMyTerminalBlock, bool> getter, Action<IMyTerminalBlock, bool> setter, Func<IMyTerminalBlock, bool> visible, string onText = "On", string offText = "Off", string tooltip = null) where T : IMyTerminalBlock
        {
            IMyTerminalControlOnOffSwitch o = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlOnOffSwitch, T>(name);
            o.Title = MyStringId.GetOrCompute(title);
            o.Tooltip = MyStringId.GetOrCompute(tooltip ?? string.Empty);
            o.OnText = MyStringId.GetOrCompute(onText);
            o.OffText = MyStringId.GetOrCompute(offText);
            o.Visible = visible;
            o.Getter = getter;
            o.Setter = setter;
            MyAPIGateway.TerminalControls.AddControl<T>(o);
            //CreateProperty(o, block);
            return o;
        }

        public static IMyTerminalControlButton AddButton<T>(string name, string title, Action<IMyTerminalBlock> action, Func<IMyTerminalBlock, bool> visible, string tooltip = null) where T : IMyTerminalBlock
        {
            IMyTerminalControlButton but = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlButton, T>(name);
            but.Title = MyStringId.GetOrCompute(title);
            but.Tooltip = MyStringId.GetOrCompute(tooltip ?? string.Empty);
            but.Action = action;
            but.Visible = visible;
            MyAPIGateway.TerminalControls.AddControl<T>(but);
            //CreateProperty(but, block);
            return but;
        }

        public static IMyTerminalControlSlider AddSlider<T>(string name, string title, Func<IMyTerminalBlock, float> getter, Action<IMyTerminalBlock, float> setter, float min, float max, Func<IMyTerminalBlock, bool> visible, string tooltip = null) where T : IMyTerminalBlock
        {
            IMyTerminalControlSlider sl = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, T>(name);
            sl.Title = MyStringId.GetOrCompute(title);
            sl.Tooltip = MyStringId.GetOrCompute(tooltip ?? string.Empty);
            sl.Visible = visible;
            sl.Getter = getter;
            sl.Setter = setter;
            sl.SetLimits(min, max);
            sl.Writer = (b, v) => v.Append($"{getter(b):N2}");
            MyAPIGateway.TerminalControls.AddControl<T>(sl);
            //CreateProperty(sl, block);
            return sl;
        }

        public static IMyTerminalControlColor AddColorEditor<T>(string name, string title, Func<IMyTerminalBlock, Color> getter, Action<IMyTerminalBlock, Color> setter, Func<IMyTerminalBlock, bool> visible) where T : IMyTerminalBlock
        {
            IMyTerminalControlColor col = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlColor, T>(name);
            col.Title = MyStringId.GetOrCompute(title);
            col.Visible = visible;
            col.Getter = getter;
            col.Setter = setter;
            MyAPIGateway.TerminalControls.AddControl<T>(col);
            //CreateProperty<T>(col);
            return col;
        }

        public static IMyTerminalControlCombobox AddCombobox<T>(string name, string title, Func<IMyTerminalBlock, long> getter, Action<IMyTerminalBlock, long> setter, Action<List<MyTerminalControlComboBoxItem>> fillAction, Func<IMyTerminalBlock, bool> visible) where T : IMyTerminalBlock
        {
            IMyTerminalControlCombobox com = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCombobox, T>(name);
            com.Title = MyStringId.GetOrCompute(title);
            com.Visible = visible;
            com.ComboBoxContent = fillAction;
            com.Getter = getter;
            com.Setter = setter;
            MyAPIGateway.TerminalControls.AddControl<T>(com);
            return com;
        }

        public static MyTerminalControlComboBoxItem AddComboboxItem(long key, string name) => new MyTerminalControlComboBoxItem { Key = key, Value = MyStringId.GetOrCompute(name) };

        public static IMyTerminalAction AddAction<T>(string id, string name, Action<IMyTerminalBlock> action, Action<IMyTerminalBlock, StringBuilder> writer, bool GroupValid = false) where T : IMyTerminalBlock
        {
            IMyTerminalAction a = MyAPIGateway.TerminalControls.CreateAction<T>(id);
            a.Name = new StringBuilder(name);
            a.Enabled = b => b.GameLogic.GetAs<MultifloorElevator>() != null;
            a.Action = action;
            a.Writer = writer;
            a.ValidForGroups = GroupValid;
            MyAPIGateway.TerminalControls.AddAction<T>(a);
            return a;
        }

        public static void CreateProperty<TValue, TBlock>(IMyTerminalValueControl<TValue> control) where TBlock : IMyTerminalBlock
        {
            IMyTerminalControlProperty<TValue> p = MyAPIGateway.TerminalControls.CreateProperty<TValue, TBlock>("MultifloorElevator." + control.Id);
            p.Visible = b => b.GameLogic.GetAs<MultifloorElevator>() != null;
            p.Enabled = b => b.GameLogic.GetAs<MultifloorElevator>() != null;
            p.Getter = control.Getter;
            p.Setter = control.Setter;
            p.SupportsMultipleBlocks = false;
            MyAPIGateway.TerminalControls.AddControl<TBlock>(p);
        }
    }
}