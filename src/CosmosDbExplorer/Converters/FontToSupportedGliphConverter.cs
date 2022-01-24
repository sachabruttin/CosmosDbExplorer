using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace CosmosDbExplorer.Converters
{
    public class FontToSupportedGliphConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not IReadOnlyCollection<FontFamily> list)
            {
                return DependencyProperty.UnsetValue;
            }

            var returnList = new List<FontFamily>();
            foreach (var font in list)
            {
                //Instantiate a TypeFace object with the font settings you want to use
                var ltypFace = new Typeface(font, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

                //Try to create a GlyphTypeface object from the TypeFace object
                if (ltypFace.TryGetGlyphTypeface(out _))
                {
                    returnList.Add(font);
                }
            }

            return returnList;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
