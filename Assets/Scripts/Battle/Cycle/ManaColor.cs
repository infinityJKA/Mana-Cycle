namespace Battle.Cycle {
    public enum ManaColor
    {
        Blue,
        Red,
        Green,
        Yellow,
        Purple,
        Multicolor,
        Colorless,
        None,
        Any, // only used by AI
        Obscured // only for networking, only sending that the tile is obscured, not the actual color
    }
}