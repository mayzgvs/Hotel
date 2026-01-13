using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Hotel
{
    /// <summary>
    /// Логика взаимодействия для AddBookingWindow.xaml
    /// </summary>
    public partial class AddBookingWindow : Window
    {
        private Entities Db => Entities.GetContext();

        public AddBookingWindow()
        {
            InitializeComponent();
            LoadCategories();
        }

        private void LoadCategories()
        {
            try
            {
                cbCategory.ItemsSource = Db.RoomCategory
                    .OrderBy(c => c.Name)
                    .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка загрузки категорий",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var fio = (tbFio.Text ?? "").Trim();
            if (string.IsNullOrWhiteSpace(fio))
            {
                MessageBox.Show("Заполни ФИО контактного лица.", "Проверка данных",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (dpFrom.SelectedDate == null || dpTo.SelectedDate == null)
            {
                MessageBox.Show("Укажи даты заезда и выезда.", "Проверка данных",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dateFrom = dpFrom.SelectedDate.Value.Date;
            var dateTo = dpTo.SelectedDate.Value.Date;

            if (dateTo <= dateFrom)
            {
                MessageBox.Show("Дата выезда должна быть больше даты заезда.", "Проверка данных",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var category = cbCategory.SelectedItem as RoomCategory;
            if (category == null)
            {
                MessageBox.Show("Выбери категорию номера.", "Проверка данных",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var booking = new Booking
                {
                    CreatedAt = DateTime.Now,
                    ContactFullName = fio,
                    ContactPhone = string.IsNullOrWhiteSpace(tbPhone.Text) ? null : tbPhone.Text.Trim(),
                    ContactEmail = string.IsNullOrWhiteSpace(tbEmail.Text) ? null : tbEmail.Text.Trim(),
                    DateFrom = dateFrom,
                    DateTo = dateTo,
                    RoomCategoryId = category.RoomCategoryId,
                    Comment = string.IsNullOrWhiteSpace(tbComment.Text) ? null : tbComment.Text.Trim()
                };

                Db.Booking.Add(booking);
                Db.SaveChanges();

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка сохранения",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
