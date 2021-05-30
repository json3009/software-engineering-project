namespace CharacterSystem
{
    public interface ICharacterStat<T> : ICharacterStat
    {
        
        /// <inheritdoc cref="ICharacterStat.Min" />
        public new T Min { get; set; }
        
        /// <inheritdoc cref="ICharacterStat.Max" />
        public new T Max { get; set; }

        /// <inheritdoc cref="ICharacterStat.Value" />
        public new T Value { get; set; }
    }

    /// <summary>
    /// Interface that represents a Character Stat.
    /// </summary>
    public interface ICharacterStat
    {
        /// <summary>
        /// The minimum for that stat.
        /// </summary>
        public object Min { get; set; }
        
        /// <summary>
        /// The maximum for that stat.
        /// </summary>
        public object Max { get; set; }
        
        /// <summary>
        /// The current value for that stat.
        /// </summary>
        public object Value { get; set; }
        
        /// <summary>
        /// Clamp the value such that <c>min &lt;= value &lt;= max</c> 
        /// </summary>
        public void Clamp();
    }
}
