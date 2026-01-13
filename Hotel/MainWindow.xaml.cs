using System;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Windows;

namespace Hotel
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Entities Db;

        public MainWindow()
        {
            InitializeComponent();
            Db = Entities.GetContext();
            LoadAll();
        }

        private void LoadAll()
        {
            try
            {
                foreach (var e in Db.ChangeTracker.Entries().ToList())
                    e.Reload();

                var list = Db.Booking
                    .Include(b => b.RoomCategory)
                    .OrderByDescending(b => b.CreatedAt)
                    .ToList();

                dgBookings.ItemsSource = list;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка загрузки",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Filter_Click(object sender, RoutedEventArgs e)
        {
            if (dpFrom.SelectedDate == null || dpTo.SelectedDate == null)
            {
                MessageBox.Show("Укажи обе даты (С и По).", "Фильтр",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var from = dpFrom.SelectedDate.Value.Date;
            var to = dpTo.SelectedDate.Value.Date;

            if (to <= from)
            {
                MessageBox.Show("Дата «По» должна быть больше даты «С».", "Фильтр",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var list = Db.Booking
                    .Include(b => b.RoomCategory)
                    .Where(b => b.DateFrom < to.AddDays(1) && b.DateTo > from)
                    .OrderBy(b => b.DateFrom)
                    .ToList();

                dgBookings.ItemsSource = list;

                if (list.Count == 0)
                    MessageBox.Show("Бронирований за период не найдено.", "Результат",
                        MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка фильтрации",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowAll_Click(object sender, RoutedEventArgs e)
        {
            LoadAll();
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            var win = new AddBookingWindow();
            win.Owner = this;

            if (win.ShowDialog() == true)
                LoadAll();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var selected = dgBookings.SelectedItem as Booking;
            if (selected == null)
            {
                MessageBox.Show("Выбери бронирование в таблице.", "Удаление",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var yes = MessageBox.Show($"Удалить бронирование №{selected.BookingId}?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (yes != MessageBoxResult.Yes) return;

            try
            {
                var entity = Db.Booking.FirstOrDefault(b => b.BookingId == selected.BookingId);
                if (entity == null)
                {
                    MessageBox.Show("Запись уже удалена или не найдена.", "Удаление",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadAll();
                    return;
                }

                Db.Booking.Remove(entity);
                Db.SaveChanges();

                LoadAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка удаления",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
