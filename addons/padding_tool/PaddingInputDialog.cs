using System;

[Tool]
public partial class PaddingInputDialog : AcceptDialog
{
    private LineEdit _lineEdit = null!;
    private Action<int>? _onConfirm;

    public override void _Ready()
    {
        Title = "Custom Padding";
        OkButtonText = "Apply";

        var vbox = new VBoxContainer();
        AddChild(vbox);

        vbox.AddChild(new Label { Text = "Padding size (px):" });

        _lineEdit = new LineEdit
        {
            Text = "8",
            PlaceholderText = "Enter padding in pixels"
        };
        vbox.AddChild(_lineEdit);

        Confirmed += OnConfirmed;
    }

    public void Show(Action<int> onConfirm)
    {
        _onConfirm = onConfirm;
        _lineEdit.Text = "8";
        PopupCentered();
    }

    private void OnConfirmed()
    {
        if (int.TryParse(_lineEdit.Text.Trim(), out var value) && value > 0)
            _onConfirm?.Invoke(value);
        else
            GD.PrintErr("Invalid padding value: must be a positive integer");
    }
}