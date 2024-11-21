namespace Game2048.Resources.Logic
{
    internal class Tile : Frame
    {
        public int row { get; set; }
        public int column { get; set; }

        public Label ValueLabel { get; set; }
        public int Value { get; set; }

        public Tile(int value, int row, int column)
        {
            CornerRadius = 10;
            BackgroundColor = GetColorByValue(value);
            WidthRequest = 95;
            HeightRequest = 95;
            HorizontalOptions = LayoutOptions.Center;
            VerticalOptions = LayoutOptions.Center;
            ZIndex = 2;

            ValueLabel = new Label
            {
                Text = value.ToString(),
                FontSize = 24,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
            };

            Value = value;
            Content = ValueLabel;

            this.row = row;
            this.column = column;
        }

        private Color GetColorByValue(int value)
        {
            if (value <= 0)
            {
                throw new ArgumentException("value has to be positive integer");
            }

            int level = (int)Math.Log(value, 2);

            int redColor = (level * 50) % 256;
            int greenColor = (level * 30) % 256;
            int blueColor = (level * 20) % 256;

            redColor = Math.Min(255, redColor + 30);
            greenColor = Math.Min(255, greenColor + 30);
            blueColor = Math.Min(255, blueColor + 30);

            return new Color(redColor, greenColor, blueColor);
        }

        public void UpdateValue(int newValue)
        {
            Value = newValue;
            ValueLabel.Text = newValue.ToString();
            BackgroundColor = GetColorByValue(newValue);
        }
    }
}
