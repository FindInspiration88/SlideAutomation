namespace SlideAutomation.Controllers
{
    public class Offset
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float StartPosX { get; set; }
        public float StartPosY { get; set; }
        private readonly float _lineWidth;
        private readonly float _maxLineLength;

        public Offset(float startX, float startY, float lineWidth,float maxLineLength)
        {
            StartPosX = startX;
            StartPosY = startY;
            _lineWidth = lineWidth;
            _maxLineLength = maxLineLength;
        }

        public void NewLine()
        {
            X = 0;
            Y += _lineWidth * 1.5F;
        }

        public void TryMakeNewLine()
        {
            if(X>_maxLineLength)
                NewLine();
        }
        public void MoveRight(float distance)
        {
            X += distance;
        }
    }
}