using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public abstract class EnemyCore : MonoBehaviour {
  [Header("Base Settings")]
  [SerializeField] GameObject enemyPrefab;
  [SerializeField] protected Transform attackPoint;
  [SerializeField] protected int maxHealth;
  [SerializeField] protected float moveSpeed;
  [SerializeField] protected float attackRange;
  [SerializeField] protected float flashDuration = 0.3f;
  [SerializeField] protected float knockbackDuration = 0.3f;
  [SerializeField] protected Color damageColor = Color.red;
  [SerializeField] protected float distanceForAttack;
  [SerializeField] protected float detectionRadius;
  [SerializeField] protected float goPatrol = 1.5f;
  [SerializeField] protected float jumpForce = 10f;
  [SerializeField] protected Transform groundCheck;
  [SerializeField] protected Transform obstacleIsFront;
  [SerializeField] protected LayerMask groundLayer;

  public bool isDie;
  protected bool goRight;
  protected SpriteRenderer spriteRenderer;
  protected Color originalColor;
  protected int currentHealth;
  protected Transform player;
  protected Rigidbody2D rb;
  protected float lastJump;


  public void Start() {
    isDie = false;
    lastJump = Time.time;
    rb = GetComponent<Rigidbody2D>();
    goRight = true;
    spriteRenderer = GetComponent<SpriteRenderer>();
    originalColor = spriteRenderer.color;
    player = GameObject.FindGameObjectWithTag("Player").transform;
    currentHealth = maxHealth;
  }

  public enum EnemyState { Patrol, Chase, Attack }
  EnemyState currentState = EnemyState.Patrol;

  public void Update() {
    if (goRight) {
      spriteRenderer.flipX = true;
    }
    else {
      spriteRenderer.flipX = false;
    }

    float targetX = goRight ? 0.7f : -0.7f;
    Vector3 newPos = obstacleIsFront.transform.localPosition;
    newPos.x = Mathf.Lerp(newPos.x, targetX, Time.deltaTime * 10f);
    obstacleIsFront.transform.localPosition = newPos;

    float targetXAttack = goRight ? 0.7f : -0.7f;
    Vector3 newPosAttack = attackPoint.transform.localPosition;
    newPosAttack.x = Mathf.Lerp(newPosAttack.x, targetXAttack, Time.deltaTime * 10f);
    attackPoint.transform.localPosition = newPosAttack;

    if (IsGrounded() && ObstacleIsFront() && Time.time > (lastJump + 1f)) {
      rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
      lastJump = Time.time;
    }

    if (currentHealth <= 0) {
      Die();
    }

    switch (currentState) {
      case EnemyState.Patrol:
        StartCoroutine(PatrolControl());
        if (Vector2.Distance(transform.position, player.transform.position) < detectionRadius) {
          currentState = EnemyState.Chase;
        }
        break;
      case EnemyState.Chase:
        Debug.Log("Враг обнаружен");
        Move();
        if (Vector2.Distance(transform.position, player.transform.position) <= distanceForAttack) {
          currentState = EnemyState.Attack;
        }
        else if (Vector2.Distance(transform.position, player.transform.position) > detectionRadius) {
          currentState = EnemyState.Patrol;
        }
        break;
      case EnemyState.Attack:
        Attack();
        if (Vector2.Distance(transform.position, player.transform.position) >= distanceForAttack) {
          currentState = EnemyState.Chase;
        }
        break;
    }
  }

  private void FixedUpdate() {
    Vector2 velocity = rb.linearVelocity;

    switch (currentState) {
      case EnemyState.Patrol:
        velocity.x = goRight ? moveSpeed : -moveSpeed;
        break;
      case EnemyState.Chase:
        Vector2 direction = (player.position - transform.position).normalized;
        velocity.x = direction.x * moveSpeed;
        break;
      case EnemyState.Attack:
        break;
    }

    rb.linearVelocity = velocity;
  }

  public virtual void TakeDamage(int damage) {
    currentHealth -= damage;
    if (currentHealth <= 0) Die();
    spriteRenderer.color = damageColor;
    CancelInvoke(nameof(ResetColor));
    Invoke(nameof(ResetColor), flashDuration);
  }

  public void ApplyKnockback(Vector2 force) {
    StartCoroutine(KnockbackRoutine(force));
  }

  IEnumerator KnockbackRoutine(Vector2 force) {
    float elapsed = 0f;
    Vector2 startPos = transform.position;
    Vector2 targetPos = startPos + force;

    while (elapsed < knockbackDuration) {
      transform.position = Vector2.Lerp(startPos, targetPos, elapsed / knockbackDuration);
      elapsed += Time.deltaTime;
      yield return null;
    }
  }
  public void ResetColor() {
    if (spriteRenderer != null) {
      spriteRenderer.color = originalColor;
    }
  }

  protected virtual IEnumerator PatrolControl() {
    if (goRight) {
      spriteRenderer.flipX = false;
      yield return new WaitForSeconds(goPatrol);
      goRight = false;
    }
    else {
      spriteRenderer.flipX = true;
      yield return new WaitForSeconds(goPatrol);
      goRight = true;
    }
  }

  public bool IsGrounded() {
    // Проверяем коллайдеры под ногами
    Collider2D[] colliders = Physics2D.OverlapCircleAll(
        groundCheck.position,
        0.4f
    );

    return colliders.Length > 0;
  }

  public bool ObstacleIsFront() {
    Vector2 direction = goRight ? Vector2.right : Vector2.left;
    RaycastHit2D hit = Physics2D.Raycast(
        obstacleIsFront.position,
        direction,
        0.5f, // Дистанция
        groundLayer
    );
    return hit.collider != null;
  }
  protected virtual void Move() {
    if ((player.position.x - transform.position.x) < 0) {
      goRight = false;
    }
    else {
      goRight = true;
    }
  }

  protected abstract void Attack();

  protected virtual void Die() {
    gameObject.SetActive(false); 
    isDie = true;
  }

  public EnemyMemento SaveState() {
    return new EnemyMemento {
      enemyPref = enemyPrefab,
      position = transform.position,
      health = currentHealth,
      currentState = currentState
    };
  }

  public void RestoreState(EnemyMemento state) {
    gameObject.SetActive(true);
    transform.position = state.position;
    currentHealth = state.health;
    currentState = state.currentState;
    isDie = false;
    ResetColor();
  }

  private void OnDrawGizmos() {
    Gizmos.color = Color.yellow;
    Gizmos.DrawWireSphere(groundCheck.position, 0.4f);
    Gizmos.color = Color.cyan;
    Gizmos.DrawRay(obstacleIsFront.position, Vector2.left);
    Gizmos.color = Color.red;
    Gizmos.DrawWireCube(attackPoint.position, attackRange * Vector2.one);
    Gizmos.color = Color.yellow;
    Gizmos.DrawWireSphere(transform.position, detectionRadius);
  }
}