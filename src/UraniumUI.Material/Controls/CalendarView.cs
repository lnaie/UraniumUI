using Microsoft.Maui.Graphics;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UraniumUI.Extensions;
using static UraniumUI.Material.Controls.DataGrid;

namespace UraniumUI.Material.Controls;

public partial class CalendarView : Border
{
    public IList<CalendarWeekColumn> Columns { get; protected set; } = new List<CalendarWeekColumn>();

    protected readonly static DateTime Today = DateTime.Now;
    protected readonly static string TodayString = Today.ToString(DayStringPattern);

    protected int Year { get; set; }
    protected int Month { get; set; }
    protected int Day { get; set; }

    protected const string DayStringPattern = "yyyy-MM-dd";

    private Grid _rootGrid;
    private Dictionary<int, IList<int[,]>> _calendarCache = new Dictionary<int, IList<int[,]>>();
    private DayOfWeek _startOfWeek = DayOfWeek.Sunday; // CultureInfo.CurrentUICulture.DateTimeFormat.FirstDayOfWeek;

    public CalendarView()
    {
        _rootGrid = new Grid
        {
            HorizontalOptions = LayoutOptions.Fill,
            Margin = new Thickness(0),
            Padding = new Thickness(0),
            ColumnSpacing = 10,
            RowSpacing = 10,
        };

        InitializeFactoryMethods();

        CalculateHighlightedDay();
        EnsureMonthIsCached(this.Year, this.Month);

        SetColumns();
        Render();
    }

    private void ResetGrid()
    {
        _rootGrid.Clear();
        _rootGrid.Children.Clear();

        _rootGrid.BackgroundColor = this.BackgroundColor;

        if (this.Content != _rootGrid)
        {
            this.Content = _rootGrid;
        }
    }

    protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        if (_rootGrid != null)
        {
            if (propertyName == nameof(this.BackgroundColor))
            {
                _rootGrid.BackgroundColor = this.BackgroundColor;
            }
        }

        base.OnPropertyChanged(propertyName);
    }

    protected void EnsureYearIsCached(int year)
    {
        if (_calendarCache.ContainsKey(year))
        {
            return;
        }

        _calendarCache[year] = new List<int[,]>(12);

        // Start from the beginning of the year
        var day = new DateTime(year, 1, 1);
        int i = 0, j = (int)day.DayOfWeek;

        while (day.Year == year)
        {
            // Create a new month view
            if (_calendarCache[year].Count < day.Month)
            {
                _calendarCache[year].Add(new int[6, 7]);
            }

            // Set the day
            _calendarCache[year][day.Month - 1][i, j] = day.Day;
            
            // Move to the next day and adjust the vars
            day = day.AddDays(1);
            j = (int)day.DayOfWeek;
            if (day.Day == 1)
            {
                i = 0; // new month
            }
            else if (j == 0)
            {
                i++;
            }
        }
    }

    protected void EnsureMonthIsCached(int year, int month)
    {
        // Ensure year
        if (!_calendarCache.ContainsKey(year))
        {
            _calendarCache[year] = new List<int[,]>(12) { null, null, null, null, null, null, null, null, null, null, null, null };
        }

        // Ensure month
        int idxMonth = month - 1;
        if (_calendarCache[year][idxMonth] != null)
        {
            return;
        }

        _calendarCache[year].Insert(idxMonth, new int[6, 7]);

        // Start from the beginning of the month
        var day = new DateTime(year, month, 1);
        var lastDay = day.AddDays(-1);
        int i = 0, j = (int)day.DayOfWeek;

        while (day.Month == month)
        {
            // Set the day
            _calendarCache[year][idxMonth][i, j] = day.Day;

            // Move to the next day and adjust the vars
            day = day.AddDays(1);
            j = (int)day.DayOfWeek;
            if (j == 0)
            {
                i++;
            }
        }

        FillTheMonthStart(lastDay, month);
        FillTheMonthEnd(day, month, i);
    }

    /// <summary>
    /// Fill the beginning of the current month in the cached calendar matrix.
    /// </summary>
    /// <param name="day">Last day of the previous month</param>
    /// <param name="month">The current month.</param>
    private void FillTheMonthStart(DateTime day, int month)
    {
        // We fill in the 1st row and we go backwards into the prev month from the end
        int idxMonth = month - 1;
        int startOfWeek = (int)_startOfWeek;

        for (var j = (int)day.DayOfWeek; j >= startOfWeek; j--)
        {
            // Set the day
            _calendarCache[day.Year][idxMonth][0, j] = day.Day;

            // Move to the next day and adjust the vars
            day = day.AddDays(-1);
        }
    }

    /// <summary>
    /// Fill the end of the current month in the cached calendar matrix.
    /// </summary>
    /// <param name="day">First day of the next month.</param>
    /// <param name="month">The current month.</param>
    /// <param name="row">The current row in the year and month  of the cached calendar matrix.</param>
    private void FillTheMonthEnd(DateTime day, int month, int row)
    {
        // We fill in the last 1-2 rows at most, and we go forwards into the next month from the beginning
        var j = (int)day.DayOfWeek;
        int idxMonth = month - 1;
        var i = row;
        
        while (i < 6)
        {
            // Set the day
            _calendarCache[day.Year][idxMonth][i, j] = day.Day;

            // Move to the next day and adjust the vars
            day = day.AddDays(1);
            j = (int)day.DayOfWeek;
            if (j == 0)
            {
                i++;
            }
        }
    }

    private void CalculateHighlightedDay()
    {
        // Should match the pattern DayStringPattern
        var parts = (SelectedDays.Count > 0 ? SelectedDays[0] : TodayString).Split(new char[] { '-' });

        Year = int.Parse(parts[0]);
        Month = int.Parse(parts[1]);
        Day = int.Parse(parts[2]);
    }

    protected virtual void Render()
    {
        if (Columns == null || Columns.Count == 0)
        {
            return; // Not ready yet.
        }

        ResetGrid();

        ConfigureGridColumnDefinitions();
        ConfigureGridRowDefinitions();

        AddTableHeaders();
        RenderCurrentView();

        RegisterSelectionChanges();
    }

    private void RenderCurrentView()
    {
        var rows = ViewType == CalendarViewType.Week ? 1 : 6;

        for (int i = 0; i < rows; i++)
        {
            AddRow(i);
        }
    }

    protected virtual void AddRow(int row)
    {
        var monthView = _calendarCache[this.Year][this.Month - 1];
        var actualRow = 1 + row; // +1 is table header.

        for (int columnNumber = 0; columnNumber < Columns.Count; columnNumber++)
        {
            var created = (View)Columns[columnNumber].CellItemTemplate?.CreateContent()
                ?? (View)CellItemTemplate?.CreateContent()
                ?? LabelFactory() ?? CreateLabel();

            created.Margin = new Thickness(0, 0, 0, 0);

            var view = new ContentView
            {
                Content = created
            };
            view.Margin = new Thickness(0, 0, 0, 0);
            view.Padding = new Thickness(0, 0, 0, 0);

            var item = monthView[row, columnNumber];
            view.BindingContext = new CellBindingContext
            {
                Column = columnNumber,
                Row = row,
                Data = item,
                Value = item,
                IsSelected = SelectedDays?.Contains($"{this.Year}-{this.Month:00}-{item:00}") ?? false
            };

            SetSelectionVisualStates(view);

            view.Triggers.Add(new DataTrigger(typeof(ContentView))
            {
                Binding = new Binding(nameof(CellBindingContext.IsSelected)),
                Value = true,
                EnterActions =
                {
                    new GoToStateTriggerAction("Selected")
                },
                ExitActions =
                {
                    new GoToStateTriggerAction("Unselected")
                }
            });

            //_rootGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            _rootGrid.Add(view, columnNumber, row: actualRow);
        }
    }

    //// https://stackoverflow.com/questions/662379/calculate-date-from-week-number
    //protected static DateTime FirstDateOfWeekISO8601(int year, int weekOfYear)
    //{
    //    DateTime jan1 = new DateTime(year, 1, 1);
    //    int daysOffset = DayOfWeek.Thursday - jan1.DayOfWeek;

    //    // Use first Thursday in January to get first week of the year as
    //    // it will never be in Week 52/53
    //    DateTime firstThursday = jan1.AddDays(daysOffset);
    //    var cal = CultureInfo.CurrentCulture.Calendar;
    //    int firstWeek = cal.GetWeekOfYear(firstThursday, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

    //    var weekNum = weekOfYear;
    //    // As we're adding days to a date in Week 1,
    //    // we need to subtract 1 in order to get the right date for week #1
    //    if (firstWeek == 1)
    //    {
    //        weekNum -= 1;
    //    }

    //    // Using the first Thursday as starting week ensures that we are starting in the right year
    //    // then we add number of weeks multiplied with days
    //    var result = firstThursday.AddDays(weekNum * 7);

    //    // Subtract 3 days from Thursday to get Monday, which is the first weekday in ISO8601
    //    return result.AddDays(-3);
    //}

    protected virtual void SetColumns()
    {
        Columns = DayOfWeekExtensions.GetWeekAsFirst(_startOfWeek)
            .Select(s => new CalendarWeekColumn
            {
                Title = s.ToString().Substring(0, 1),
            }).ToList();
    }

    private void ConfigureGridColumnDefinitions()
    {
        _rootGrid.ColumnDefinitions.Clear();

        for (int i = 0; i < Columns.Count; i++)
        {
            _rootGrid.AddColumnDefinition(new ColumnDefinition(Columns[i].Width));
        }
    }

    private void ConfigureGridRowDefinitions()
    {
        _rootGrid.RowDefinitions.Clear();

        var actualRows = ViewType == CalendarViewType.Week ? 2 : 7;
        for (int i = 0; i < actualRows; i++)
        {
            _rootGrid.AddRowDefinition(new RowDefinition(GridLength.Auto));
        }
    }

    protected virtual void AddTableHeaders()
    {
        for (int i = 0; i < Columns.Count; i++)
        {
            var label = LabelFactory() ?? CreateLabel();
            label.FontAttributes = FontAttributes.Bold;
            // TODO: Use an attribute to localize it.
            label.BindingContext = new CellBindingContext
            {
                Value = Columns[i].Title
            };
            label.Margin = new Thickness(0);
            label.Padding = new Thickness(0);

            _rootGrid.Add(label, column: i, row: 0);
        }
    }

    private void RegisterSelectionChanges()
    {
        foreach (IDataGridSelectionColumn selection in Columns.Where(x => x is IDataGridSelectionColumn))
        {
            selection.SelectionChanged += SelectionChanged;
        }
    }

    private void SelectionChanged(object sender, bool isSelected)
    {
        if (sender is View view && view.BindingContext is CellBindingContext cellContext)
        {
            if (isSelected)
            {
                SelectedDays?.Add(cellContext.Data.ToString());
            }
            else
            {
                SelectedDays?.Remove(cellContext.Data.ToString());
            }

            OnPropertyChanged(nameof(SelectedDays));
        }
    }

    protected virtual void OnSelectedItemsSet()
    {
        UpdateSelections();

        if (SelectedDays is INotifyCollectionChanged observable)
        {
            observable.CollectionChanged += (s, e) => UpdateSelections();
        }
    }

    protected void UpdateSelections()
    {
        foreach (View child in _rootGrid.Children)
        {
            if (child.BindingContext is CellBindingContext cellBindingContext)
            {
                cellBindingContext.IsSelected = SelectedDays.Contains(cellBindingContext.Data.ToString());
            }
        }
    }

    protected void SetSelectionVisualStatesForAll()
    {
        if (_rootGrid is null)
        {
            return;
        }

        foreach (View child in _rootGrid.Children)
        {
            SetSelectionVisualStates(child);
        }
    }

    protected virtual void SetSelectionVisualStates(View view)
    {
        VisualStateManager.SetVisualStateGroups(view, new VisualStateGroupList
        {
            new VisualStateGroup
            {
                Name = "CalendarSelectionStates",
                States =
                {
                    new VisualState
                    {
                        Name = "Selected",
                        Setters =
                        {
                            new Setter
                            {
                                Property = View.BackgroundColorProperty,
                                Value = SelectionColor.MultiplyAlpha(0.2f)
                            }
                        }
                    },
                    new VisualState
                    {
                        Name = "Unselected",
                        Setters =
                        {
                            new Setter
                            {
                                Property = View.BackgroundColorProperty,
                                Value = Colors.Transparent
                            }
                        }
                    }
                }
            }
        });
    }

    //private void OnItemSourceSet(IList oldSource, IList newSource)
    //{
    //    var sourceType = newSource.GetType();
    //    if (sourceType.GenericTypeArguments.Length != 1)
    //    {
    //        throw new InvalidOperationException("DataGrid collection must be a generic typed collection like List<T>.");
    //    }

    //    CurrentType = sourceType.GenericTypeArguments.First();

    //    var columnsAreReady = Columns?.Any() ?? false;

    //    SetAutoColumns();

    //    if (oldSource is INotifyCollectionChanged oldObservable)
    //    {
    //        oldObservable.CollectionChanged -= ItemsSource_CollectionChanged;
    //    }

    //    if (newSource is INotifyCollectionChanged newObservable)
    //    {
    //        newObservable.CollectionChanged += ItemsSource_CollectionChanged;
    //    }

    //    if (columnsAreReady)
    //    {
    //        Render();
    //    }
    //}
}

public class CalendarDay
{
    /// <summary>
    /// The day of the month in "yyyy-MM-dd" format.
    /// </summary>
    public string Title { get; set; }
    public int WeekNumber { get; set; }
}

public class CalendarWeekColumn
{
    public string Title { get; set; }

    [TypeConverter(typeof(GridLengthTypeConverter))]
    public GridLength Width { get; set; } = GridLength.Auto;

    public DataTemplate CellItemTemplate { get; set; }
}

public enum CalendarViewType
{
    Month = 0,
    Week
}