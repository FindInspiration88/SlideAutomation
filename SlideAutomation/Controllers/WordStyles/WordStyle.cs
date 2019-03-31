using System.Drawing;

namespace SlideAutomation.Controllers.WordStyles
{

    public class WordStyle
    {
        public Font Font { get; }
        public SolidBrush SolidBrush { get; }

        public WordStyle(Font font, SolidBrush solidBrush)
        {
            Font = font;
            SolidBrush = solidBrush;
        }

        public override bool Equals(object obj)
        {
            var objAsWordStyle = obj as WordStyle;
            return objAsWordStyle != null &&
                   objAsWordStyle.Font.Equals(Font) &&
                   objAsWordStyle.SolidBrush.Color.Equals(SolidBrush.Color);
        }

        public override int GetHashCode()
        {
            return Font.GetHashCode() * SolidBrush.GetHashCode();
        }
    }
}