using UnityEngine;

namespace AISystem.Decisions
{
    /// <summary>
    /// <para>Checks whether the <see cref="AIStateController">state machine</see> object has left the range of the target position.</para>
    /// <inheritdoc path="/summary"/>
    /// </summary>
    [CreateAssetMenu (menuName = "Game/AI/Decisions/Target Left")]
    public class TargetLeft : AIDecision
    {
        [SerializeField] private float _distance = 1;
        public override bool Decide(AIStateController controller)
        {
            if (!controller.TargetPosition.HasValue)
                return true;
                
            if (Vector2.Distance(controller.TargetPosition.Value, controller.transform.position) > _distance)
                return true;

            return false;
        }
    }
}