namespace Game.Sim
{
    public enum AntState
    {
        /// <summary>Walking out to a dirt tile.</summary>
        ToDig,
        /// <summary>Standing next to the target tile, filling its jaws.</summary>
        Digging,
        /// <summary>Carrying dirt back to the base.</summary>
        Returning,
        /// <summary>Dropping the load at the base.</summary>
        Depositing,
        /// <summary>No reachable dirt; wandering near the base until some appears.</summary>
        Idle,
        /// <summary>Walled in with no open path home; tunnelling straight toward the base.</summary>
        DiggingOut,
    }
}
