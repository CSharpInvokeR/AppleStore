using System;
using System.Windows;
using System.Windows.Controls;

namespace AppleStore.Pages
{
    public partial class FormatSelectionWindow : Window
    {
        public string SelectedFormat { get; private set; }
        public string ReportName { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public bool ShowPeriod { get; set; }

        public FormatSelectionWindow(string defaultName = "", bool showPeriod = true)
        {
            InitializeComponent();
            txtReportName.Text = defaultName;
            txtReportName.SelectAll();
            txtReportName.Focus();

            ShowPeriod = showPeriod;
            periodPanel.Visibility = showPeriod ? Visibility.Visible : Visibility.Collapsed;

            if (showPeriod)
            {
                DateTime today = DateTime.Now.Date;
                DateTime startDate = today.AddMonths(-1);

                dpStartDate.SelectedDate = startDate;
                dpEndDate.SelectedDate = today;

                // Ограничиваем выбор дат - нельзя выбрать позже сегодня
                dpStartDate.BlackoutDates.Add(new CalendarDateRange(today.AddDays(1), DateTime.MaxValue));
                dpEndDate.BlackoutDates.Add(new CalendarDateRange(today.AddDays(1), DateTime.MaxValue));

                // Также можно ограничить выбор дат до сегодня для start
                dpStartDate.BlackoutDates.Add(new CalendarDateRange(today.AddDays(1), DateTime.MaxValue));
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtReportName.Text))
            {
                MessageBox.Show("Введите название отчета", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtReportName.Focus();
                return;
            }

            if (ShowPeriod)
            {
                if (!dpStartDate.SelectedDate.HasValue || !dpEndDate.SelectedDate.HasValue)
                {
                    MessageBox.Show("Выберите период отчета", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                StartDate = dpStartDate.SelectedDate.Value;
                EndDate = dpEndDate.SelectedDate.Value;

                if (StartDate > EndDate)
                {
                    MessageBox.Show("Дата начала не может быть позже даты окончания", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (EndDate > DateTime.Now.Date)
                {
                    MessageBox.Show("Нельзя выбрать будущую дату", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            ReportName = txtReportName.Text.Trim();

            if (rbWord.IsChecked == true)
                SelectedFormat = "Word";
            else if (rbExcel.IsChecked == true)
                SelectedFormat = "Excel";

            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}