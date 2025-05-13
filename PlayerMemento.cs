using UnityEngine;

public class PlayerMemento {
  public Vector3 position;
  public int health;       

  public PlayerMemento(Vector3 pos, int hp) {
    position = pos;
    health = hp;
  }
}
