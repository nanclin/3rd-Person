using UnityEngine;

[CreateAssetMenu(fileName = "LineAttractorData", menuName = "Anclin/LineAttractorData", order = 1)]
public class LineAttractorData : ScriptableObject {
    public AnimationCurve Curve;
    [Range(0, 5)] public float Range = 2;
    [Range(0, 1)] public float Power = .5f;
}