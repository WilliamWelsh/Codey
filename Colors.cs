using Discord;

namespace Codey
{
    public static class Colors
    {
        public static Color Green = new Color(46, 204, 113);
        public static Color Blue = new Color(52, 152, 219);
        public static Color Purple = new Color(155, 89, 182);
        public static Color Yellow = new Color(241, 196, 15);
        public static Color Orange = new Color(230, 126, 34);
        public static Color Red = new Color(231, 76, 60);
        public static Color White = new Color(255, 255, 255);
        public static Color Black = new Color(0, 0, 0);
        public static Color Gray = new Color(149, 165, 166);

        public static Color GetColorFromString(string color)
        {
            switch (color.ToLower())
            {
                case "green":
                    return Green;

                case "blue":
                    return Blue;

                case "purple":
                    return Purple;

                case "yellow":
                    return Yellow;

                case "orange":
                    return Orange;

                case "red":
                    return Red;

                case "white":
                    return White;

                case "black":
                    return Black;

                case "grey":
                case "gray":
                    return Gray;

                default:
                    return White;
            }
        }
    }
}