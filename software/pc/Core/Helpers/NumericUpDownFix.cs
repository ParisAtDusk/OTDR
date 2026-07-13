using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace OTDR.Core.Helpers;

public static class NumericUpDownFix
{
    public static readonly AttachedProperty<bool> AcceptCommaProperty =
        AvaloniaProperty.RegisterAttached<Control, bool>("AcceptComma", typeof(NumericUpDownFix));

    public static readonly AttachedProperty<decimal> DefaultValueProperty =
        AvaloniaProperty.RegisterAttached<Control, decimal>("DefaultValue", typeof(NumericUpDownFix), 1.0m);

    static NumericUpDownFix()
    {
        AcceptCommaProperty.Changed.AddClassHandler<NumericUpDown>((nud, _) =>
        {
            nud.TemplateApplied += (_, args) =>
            {
                var tb = args.NameScope.Find<TextBox>("PART_TextBox");
                if (tb is null) return;

                tb.AddHandler(InputElement.TextInputEvent, (_, e) =>
                {
                    if (e is TextInputEventArgs tie && tie.Text == ",")
                        tie.Text = ".";
                }, RoutingStrategies.Tunnel);

                tb.LostFocus += (_, _) =>
                {
                    if (nud.Value is null)
                    {
                        var def = nud.GetValue(DefaultValueProperty);
                        nud.Value = def;
                    }
                };
            };
        });
    }

    public static void SetAcceptComma(NumericUpDown obj, bool value) =>
        obj.SetValue(AcceptCommaProperty, value);

    public static bool GetAcceptComma(NumericUpDown obj) =>
        obj.GetValue(AcceptCommaProperty);

    public static void SetDefaultValue(NumericUpDown obj, decimal value) =>
        obj.SetValue(DefaultValueProperty, value);

    public static decimal GetDefaultValue(NumericUpDown obj) =>
        obj.GetValue(DefaultValueProperty);
}