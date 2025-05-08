using UnityEngine;

public class Checkpoint : MonoBehaviour {
  private CheckPointManager manager;
  private BoxCollider2D col;

  void Start() {
    col = GetComponent<BoxCollider2D>();
    manager = FindAnyObjectByType<CheckPointManager>();
  }

  void OnTriggerEnter2D(Collider2D other) {
    if (other.CompareTag("Player")) {
      manager.SaveCheckpoint();
      GetComponent<SpriteRenderer>().color = Color.green; // Визуальная обратная связь
      Destroy(col);
    }
  }
}
