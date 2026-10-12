namespace AniBato.Game
{
    public readonly struct MenuInput
    {
        public readonly int Horizontal, Vertical;
        public readonly bool Confirm, Cancel, Start;
        public MenuInput(int x, int y, bool confirm, bool cancel, bool start)
        { Horizontal = x; Vertical = y; Confirm = confirm; Cancel = cancel; Start = start; }
    }
}
