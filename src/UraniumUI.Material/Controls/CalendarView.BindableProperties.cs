using InputKit.Shared;
using System.Collections;
using System.Collections.ObjectModel;

namespace UraniumUI.Material.Controls
{
    public partial class CalendarView
    {
        public Color LineGripSeperatorColor { get => (Color)GetValue(LineGripSeperatorColorProperty); set => SetValue(LineGripSeperatorColorProperty, value); }
        public static readonly BindableProperty LineGripSeperatorColorProperty =
            BindableProperty.Create(nameof(LineGripSeperatorColor), typeof(Color), typeof(CalendarView), defaultValue: Colors.Gray, propertyChanged: (bo, ov, nv) => (bo as CalendarView).OnPropertyChanged(nameof(LineGripSeperatorColor)));

        public IList<string> SelectedDays { get => (IList<string>)GetValue(SelectedDaysProperty); set => SetValue(SelectedDaysProperty, value); }
        public static readonly BindableProperty SelectedDaysProperty =
            BindableProperty.Create(nameof(SelectedDays), typeof(IList<string>), typeof(CalendarView), defaultValue: new List<string>(0), defaultBindingMode: BindingMode.TwoWay, propertyChanged: (bo, ov, nv) => (bo as CalendarView).OnSelectedItemsSet());

        //public string SelectedDay { get => (string)GetValue(SelectedDayProperty); set => SetValue(SelectedDayProperty, value); }
        //public static readonly BindableProperty SelectedDayProperty =
        //    BindableProperty.Create(nameof(SelectedDay), typeof(string), typeof(CalendarView), defaultValue: Today.ToString(DayStringPattern), propertyChanged: (bo, ov, nv) => (bo as CalendarView).OnPropertyChanged(nameof(SelectedDay)));

        //public string DisplayedDay { get => (string)GetValue(DisplayedDayProperty); set => SetValue(DisplayedDayProperty, value); }
        //public static readonly BindableProperty DisplayedDayProperty =
        //    BindableProperty.Create(nameof(DisplayedDay), typeof(string), typeof(CalendarView), defaultValue: Today.ToString(DayStringPattern), propertyChanged: (bo, ov, nv) => (bo as CalendarView).OnPropertyChanged(nameof(DisplayedDay)));

        public CalendarViewType ViewType { get => (CalendarViewType)GetValue(ViewTypeProperty); set => SetValue(ViewTypeProperty, value); }
        public static readonly BindableProperty ViewTypeProperty =
            BindableProperty.Create(nameof(ViewType), typeof(CalendarViewType), typeof(CalendarView), defaultValue: CalendarViewType.Month, propertyChanged: (bo, ov, nv) => (bo as CalendarView).OnPropertyChanged(nameof(ViewType)));

        public Color SelectionColor { get => (Color)GetValue(SelectionColorProperty); set => SetValue(SelectionColorProperty, value); }
        public static readonly BindableProperty SelectionColorProperty =
            BindableProperty.Create(nameof(SelectionColor), typeof(Color), typeof(CalendarView), defaultValue: InputKitOptions.GetAccentColor(),
                propertyChanged: (bo, ov, nv) => (bo as CalendarView).SetSelectionVisualStatesForAll());

        public DataTemplate CellItemTemplate { get => (DataTemplate)GetValue(CellItemTemplateProperty); set => SetValue(CellItemTemplateProperty, value); }
        public static readonly BindableProperty CellItemTemplateProperty =
            BindableProperty.Create(nameof(CellItemTemplate), typeof(DataTemplate), typeof(CalendarView), defaultValue: null,
                propertyChanged: (bo, ov, nv) => (bo as CalendarView).Render());
    }
}
