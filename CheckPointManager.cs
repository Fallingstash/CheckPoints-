using UnityEngine;

public class CheckPointManager : MonoBehaviour
{
  private PlayerMemento lastCheckpoint;
  private PlayerController player;

  void Start() {
    player = FindAnyObjectByType<PlayerController>();
  }

  public void SaveCheckpoint() {
    lastCheckpoint = player.SaveState();
    Debug.Log("Checkpoint saved at: " + lastCheckpoint.position);
  }

  public void LoadCheckpoint() {
    if (lastCheckpoint != null) {
      player.RestoreState(lastCheckpoint);
    }
  }
}
