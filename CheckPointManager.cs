using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class CheckPointManager : MonoBehaviour {
  private PlayerMemento playerMemento;
  private EnemyMemento[] enemyMemento;
  private PlayerController player;
  private EnemyCore[] enemies;

  void Start() {
    player = FindAnyObjectByType<PlayerController>();
    // Ищем ВСЕХ врагов, включая деактивированных
    enemies = FindObjectsOfType<EnemyCore>(true);
    enemyMemento = new EnemyMemento[enemies.Length];
  }

  public void SaveCheckpoint() {
    // Сохраняем игрока
    playerMemento = player.SaveState();

    int count = 0;
    for (int countOfEnemies = 0; countOfEnemies < enemies.Length; ++countOfEnemies) {
      enemyMemento[count] = enemies[count].SaveState();
      ++count;
    }

    Debug.Log("Checkpoint saved!");
  }

  public void LoadCheckpoint() {
    player.RestoreState(playerMemento);

    enemies = FindObjectsOfType<EnemyCore>(true);

    for (int countOfEnemies = 0; countOfEnemies < enemies.Length; ++countOfEnemies) {
      if (enemies[countOfEnemies] == null) {
        GameObject newEnemy = Instantiate(enemyMemento[countOfEnemies].enemyPref,
                                        enemyMemento[countOfEnemies].position,
                                        Quaternion.identity);
        newEnemy.GetComponent<EnemyCore>().RestoreState(enemyMemento[countOfEnemies]);
      }
      else {
        enemies[countOfEnemies].RestoreState(enemyMemento[countOfEnemies]);
      }
    }

    Time.timeScale = 1;
  }
}