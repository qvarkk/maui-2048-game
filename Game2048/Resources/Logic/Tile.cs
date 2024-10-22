namespace Game2048.Resources.Logic
{
    internal class Tile : Frame
    {
        public int row { get; set; }
        public int column { get; set; }

        private Dictionary<int, Color> tilesValuesColors = new Dictionary<int, Color>()
            {
                { 2, new Color(238, 228, 218) },
                { 4, new Color(237, 224, 200) },
                { 8, new Color(242, 177, 121) },
                { 16, new Color(245, 149, 99) },
                { 32, new Color(246, 124, 95) },
                { 64, new Color(246, 94, 59) },
                { 128, new Color(237, 207, 114) },
                { 256, new Color(237, 204, 97) },
                { 512, new Color(237, 200, 80) },
                { 1024, new Color(237, 197, 63) },
                { 2048, new Color(237, 194, 46) }
            };

        public Label ValueLabel { get; set; }
        public int Value { get; set; }

        public Tile(int value, int row, int column)
        {
            CornerRadius = 10;
            BackgroundColor = tilesValuesColors[value];
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

        public void UpdateValue(int newValue)
        {
            Value = newValue;
            ValueLabel.Text = newValue.ToString();
            BackgroundColor = tilesValuesColors[newValue];
        }
    }
}
