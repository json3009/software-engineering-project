using UnityEngine;

namespace AISystem.Decisions
{
    /// <summary>
    /// <para>Checks whether the <see cref="AIStateController">state machine</see> sees the <see cref="AIStateController.TargetObject">target character</see>(often the player).</para>
    /// <inheritdoc path="/summary"/>
    /// </summary>
    [CreateAssetMenu (menuName = "Game/AI/Decisions/Scan Area")]
    public class ScanArea : AIDecision
    {
        [SerializeField] private float _margin;
        public override bool Decide(AIStateController controller)
        {
            // check area!
            if (Vector2.Distance(controller.TargetObject.transform.position, controller.transform.position) < _margin)
                return true;

            return false;
        }
    }
}