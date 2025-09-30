using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlidingPuzzleGame
{
    public class GameSquare : Label
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public int Index { get; set; }
        public char TextValue { get; set; }
        public char WinText { get; set; }

        public GameSquare(char textValue, char winText, int index)
        {
            TextValue = textValue;
            WinText = winText;
            Index = index;
            Text = textValue.ToString();
            BackgroundColor = Colors.LightBlue;
            HorizontalTextAlignment = TextAlignment.Center;
            VerticalTextAlignment = TextAlignment.Center;
        }

        public Task AnimateWinAsync (bool reset)
        {
            this.Text = reset ? TextValue.ToString() : WinText.ToString();
            return this.ScaleTo(reset ? 1 : 1.2, 100);
        }

        public void SetLabelFont(double fontSize, FontAttributes fontAttributes)
        {
            FontSize = fontSize;
            FontAttributes = fontAttributes;
        }
    }
}
