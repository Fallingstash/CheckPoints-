using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour {
  [Header("Double Jump")]
  [SerializeField] private int maxJumps = 2;         // Максимум прыжков (2 = двойной)
  private int jumpsRemaining;          // Оставшиеся прыжки

  [Header("Movement")]
  [SerializeField] private float moveSpeed = 5f;
  [SerializeField] private float jumpForce = 30f;

  [Header("Ground Check")]
  [SerializeField] private Transform groundCheck;
  [SerializeField] private LayerMask groundLayer;
  [SerializeField] private float checkRadius = 0.2f;

  [Header("Attack")]
  [SerializeField] private AttackZone attackZone;
  [SerializeField] private float attackDuration = 0.3f;

  private bool canAttack = true; 
  public bool isFacingRight = true;

  // === Компоненты === //
  private Rigidbody2D rb;
  private SpriteRenderer sprite;

  // === Ввод === //
  private float moveInput;
  private bool isJumpPressed;

  void Awake() {
    // Кэшируем компоненты
    rb = GetComponent<Rigidbody2D>();
    sprite = GetComponent<SpriteRenderer>();

    // Проверяем настройки
    if (groundCheck == null) {
      Debug.LogError("Не назначен GroundCheck!");
    }
  }

  void Update() {
    // Получаем ввод с клавиатуры
    moveInput = Input.GetAxisRaw("Horizontal");

    if (Input.GetButtonDown("Fire1") && canAttack) {
      StartCoroutine(Attack());
    }

    // Прыжок (проверяем землю или возможность двойного прыжка)
    if (Input.GetButtonDown("Jump")) {
      if (IsGrounded()) {
        jumpsRemaining = maxJumps; // Сброс при приземлении
      }

      if (jumpsRemaining > 0) {
        isJumpPressed = true;
        jumpsRemaining--;
      }
    }

    // Поворот спрайта
    if (moveInput > 0.01f) {
      sprite.flipX = false;
      isFacingRight = true;
      
    }
    else if (moveInput < -0.01f) {
      sprite.flipX = true;
      isFacingRight = false;
    }

    // Плавное движение зоны к целевой позиции
    float targetX = isFacingRight ? 0.7f : -0.7f;
    Vector3 newPos = attackZone.transform.localPosition;
    newPos.x = Mathf.Lerp(newPos.x, targetX, Time.deltaTime * 10f);
    attackZone.transform.localPosition = newPos;

  }

  public void FixedUpdate() {
    // Движение (через velocity для плавности)
    rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocityY);

    // Прыжок (через AddForce)
    if (isJumpPressed) {
      rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
      isJumpPressed = false;
    }
  }

  public IEnumerator Attack() {
    canAttack = false;
    attackZone.ToggleAttack(true); // Включаем зону атаки

    yield return new WaitForSeconds(attackDuration);

    attackZone.ToggleAttack(false); // Выключаем зону
    canAttack = true;
  }

  public bool IsGrounded() {
    // Проверяем коллайдеры под ногами
    Collider2D[] colliders = Physics2D.OverlapCircleAll(
        groundCheck.position,
        checkRadius
        
    );

    return colliders.Length > 0;
  }

  public PlayerMemento SaveState() {
    GetComponent<PlayerHealth>().respawnPoint = transform.position;
    return new PlayerMemento(transform.position, GetComponent<PlayerHealth>().currentHealth);
  }

  public void RestoreState(PlayerMemento memento) {
    transform.position = memento.position;
    GetComponent<PlayerHealth>().SetHealth(memento.health);
    GetComponent<PlayerHealth>().isDeath = false;
    // Дополнительные действия (например, сброс анимаций)
  }

  // Визуализация в редакторе
  void OnDrawGizmos() {
    Gizmos.color = Color.yellow;
    Gizmos.DrawWireSphere(
        groundCheck.position,
        checkRadius
    );
  }
}