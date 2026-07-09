using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace AppleStore.Converter
{
    public class ImagePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string defaultImage = "pack://application:,,,/Resources/picture.png";

            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return new BitmapImage(new Uri(defaultImage));
            }

            string fileName = value.ToString();
            string imagePath = $"pack://application:,,,/Resources/{fileName}";

            try
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri(imagePath);
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                image.Freeze();
                return image;
            }
            catch
            {
                return new BitmapImage(new Uri(defaultImage));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}