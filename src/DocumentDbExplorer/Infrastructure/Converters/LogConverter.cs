using System;
using System.Globalization;
using System.Windows.Data;

namespace DocumentDbExplorer.Infrastructure.Converters
{
    [ValueConversion(typeof(double), typeof(double))]
    public class LogConverter : IValueConverter
    {
        private readonly double _a;
        private readonly double _b;
        private readonly double _c;

        public LogConverter()
        {
            const int x = 0;
            const int y = 100;
            const int z = 400;

            _a = GetA(x, y, z);
            _b = GetB(x, y, z);
            _c = GetC(x, y, z);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return _b * (Math.Exp(_c * (double)value) - 1.0) / 100;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //return _b * (Math.Exp(_c * (double)value) - 1.0);
            return Binding.DoNothing;
            //return Math.Log(((double)value - _a) / _b) / _c;
            //return value;
        }

        private double GetA(double x, double y, double z)
        {
            return ((x * z) - Math.Pow(y, 2)) / (x - (2 * y) + z);
        }

        private double GetB(double x, double y, double z)
        {
            return Math.Pow(y - x, 2) / (x - (2 * y) + z);
        }

        private double GetC(double x, double y, double z)
        {
            return 2 * Math.Log((z - y) / (y - x));
        }
    }
}
