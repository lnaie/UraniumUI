using System;

namespace UraniumUI.Material.Controls;

public partial class CalendarView
{
    public Func<Label> LabelFactory { get; set; }

    public Func<View> HorizontalLineFactory { get; set; }

    private void InitializeFactoryMethods()
    {
        LabelFactory = CreateLabel;
        HorizontalLineFactory = CreateHorizontalLineGrip;
    }

    protected virtual Label CreateLabel()
    {
        var label = new Label
        {
            Margin = 20,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        label.SetBinding(Label.TextProperty, "Value");

        return label;
    }

    protected virtual View CreateHorizontalLineGrip()
    {
        var boxView = new BoxView
        {
            HorizontalOptions = LayoutOptions.Fill,
            HeightRequest = 2,
            CornerRadius = 1,
            Opacity = .4
        };

        boxView.SetBinding(BoxView.ColorProperty, new Binding(nameof(LineGripSeperatorColor), source: this));

        return boxView;
    }
}

