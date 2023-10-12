using UnityEngine;

namespace CustomShips {
    public class PlacementBlock : MonoBehaviour {
        public static Player.PlacementStatus invalidRipPlacement = (Player.PlacementStatus)"MS_InvalidRipPlacement".GetStableHashCode();

        public PlacementRule placementRule;

        public void CheckRibPlacement(Player player) {
            player.m_tempPieces.Clear();
            player.FindClosestSnapPoints(player.m_placementGhost.transform, 0.5f, out Transform selfSnapPoint, out Transform otherSnapPoint, player.m_tempPieces);

            if (!otherSnapPoint || !otherSnapPoint.parent.GetComponent<Keel>()) {
                player.m_placementStatus = invalidRipPlacement;
            }
        }
    }
}
