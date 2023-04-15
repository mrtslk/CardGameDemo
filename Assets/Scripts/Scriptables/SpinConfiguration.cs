using UnityEngine;

public class SpinConfiguration : ScriptableObject
{
    #region Variables & Properties
    [SerializeField, Tooltip("Settings for Bronze Spin Wheel")]
    private WheelConfig bronzeWheel;
    [SerializeField, Tooltip("Settings for Silver Spin Wheel")]
    private WheelConfig silverWheel;
    [SerializeField, Tooltip("Settings for Golden Spin Wheel")]
    private WheelConfig goldenWheel;
    #endregion

    #region Public Methods
    public WheelConfig GetConfig(SpinTypes spinType)
    {
        return spinType switch
        {
            SpinTypes.Bronze => bronzeWheel,
            SpinTypes.Silver => silverWheel,
            SpinTypes.Golden => goldenWheel,
            _ => null,
        };
    }
    #endregion
}
