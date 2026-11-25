using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using UtilityBills.Core.Billing;
using UtilityBills.Core.Common;
using UtilityBills.Core.Meters;
using UtilityBills.Core.Orders;
using UtilityBills.Core.Storage;

namespace UtilityBills.Gui;

public partial class MainWindow : Window
{
    private readonly IMeterStorage _storage;
    private readonly List<Meter> _allMeters;
    private List<Meter> _displayedMeters = new();
    private MonthlySummary _summary = new(0, 0, 0);

    public MainWindow()
    {
        InitializeComponent();

        var dataPath = Path.Combine(AppContext.BaseDirectory, "data", "meters.json");
        _storage = new JsonMeterStorage(dataPath);
        _allMeters = _storage.Load();

        if (_allMeters.Count == 0)
        {
            _allMeters = new List<Meter>
            {
                new SingleZoneMeter(1, "Холодна вода (кухня)", 31.20, 410, "м³")
                    { ServiceType = "Водопостачання" },
                new DualZoneMeter(2, "Електроенергія (день/ніч)", 4.32, 2.16, 1200, 600, "День (Т1)", "Ніч (Т2)")
                    { ServiceType = "Електрика" },
                new StandingChargeMeter(3, "Вивіз побутових відходів", 45.50, 2)
                    { ServiceType = "Утримання будинку" },
                new SingleZoneMeter(4, "Газ (плита та колонка)", 7.96, 890, "м³")
                    { ServiceType = "Газопостачання" },
                new ThreeZoneMeter(5, "Електроопалення (3 зони)", 6.48, 4.32, 1.73, 500, 800, 400)
                    { ServiceType = "Електрика" }
            };
            _storage.Save(_allMeters);
        }

        var filter = new CatalogFilter<Meter>();
        var meteredCount = filter.CountMatching(_allMeters, m => m is not StandingChargeMeter);
        StatusText.Text = $"Послуг у базі: {_allMeters.Count} (із лічильниками показань: {meteredCount})";

        RefreshList();
    }

    private void RefreshList()
    {
        if (MetersListBox is null)
            return;

        IBillingOrder order = OrderComboBox?.SelectedIndex == 1
            ? new HighestRateFirstOrder()
            : new SequentialOrder();

        _displayedMeters = order.Arrange(_allMeters).ToList();
        MetersListBox.ItemsSource = _displayedMeters;
        if (_displayedMeters.Count > 0)
            MetersListBox.SelectedIndex = 0;
    }

    private void OrderComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        RefreshList();
    }

    private void MetersListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (MetersListBox.SelectedItem is not Meter meter)
            return;

        SelectedTitleText.Text = meter.ToString();
        TariffDetailsText.Text = meter.TariffDetails;
        PromptText.Text = $"{meter.Prompt}:";
        ReadingTextBox.Text = "";
        BillResultText.Text = "";
    }

    private void CalculateButton_Click(object? sender, RoutedEventArgs e)
    {
        if (MetersListBox.SelectedItem is not Meter meter)
            return;

        var input = ReadingTextBox.Text ?? "";
        var bill = meter.CalculateBill(input);
        _summary += bill;

        if (bill >= 0)
        {
            BillResultText.Foreground = Brushes.ForestGreen;
            BillResultText.Text = $"Нараховано: {bill:F2} грн";
        }
        else
        {
            BillResultText.Foreground = Brushes.Crimson;
            BillResultText.Text = "Помилка: перевірте формат або значення показань.";
        }

        UpdateSummaryText();
    }

    private void ResetButton_Click(object? sender, RoutedEventArgs e)
    {
        _summary = new MonthlySummary(0, 0, 0);
        BillResultText.Text = "";
        UpdateSummaryText();
    }

    private void UpdateSummaryText()
    {
        SummaryText.Text =
            $"Підсумок: опрацьовано {_summary.ValidCount} з {_summary.ProcessedCount}, " +
            $"до сплати: {_summary.TotalAmount:F2} грн (середній чек: {_summary.AverageBill:F2} грн)";
    }
}