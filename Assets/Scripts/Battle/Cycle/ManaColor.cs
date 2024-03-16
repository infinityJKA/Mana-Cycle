namespace Battle.Cycle {
    // OLD ENUM SYSTEM
    // public enum ManaColor
    // {
    //     Blue,
    //     Red,
    //     Green,
    //     Yellow,
    //     Purple,
    //     Multicolor,
    //     Colorless,
    //     None,
    //     Any, // only used by AI
    //     Obscured // only for networking, only sending that the tile is obscured, not the actual color
    // }

    // new system - negative numbers represent special tags
    public static class ManaColor 
    {
        /// <summary>
        /// used to represent any valid color. (used by blob detection, etc)
        /// used to denote prioritizing any color in the AI placement job as opposed to a specific color
        /// </summary>
        public const int Any = -1;

        // multicolor - alias for Any. used in geo gold mine but identical functionality & has a clearer meaning in the code
        public const int Multicolor = -1;

        /// <summary>
        /// don't match any color, or a tile with no color.
        /// </summary>
        public const int None = -2;

        /// <summary>
        /// i forgor what this means, might remove it
        /// </summary>
        public const int Colorless = -3;
    }
}