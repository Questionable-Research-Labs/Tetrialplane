using UnityEngine;

namespace Scripts {
    public class DynamicMeshColliderManager : MonoBehaviour {
        public void RecalculateMeshes() {
            foreach (var boxCollider in GetComponents<BoxCollider>()) {
                Destroy(boxCollider);
            }
            
            foreach (Transform box in transform) { 
                var boxCollider = gameObject.AddComponent<BoxCollider>();
                boxCollider.center = box.localPosition;
                boxCollider.size = box.localScale;
            }
        }

        private void Awake() {
            RecalculateMeshes();
        }
    }
}