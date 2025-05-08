using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour {
  [Header("Double Jump")]
  [SerializeField] private int maxJumps = 2;         // �������� ������� (2 = �������)
  private int jumpsRemaining;          // ���������� ������

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

  // === ���������� === //
  private Rigidbody2D rb;
  private SpriteRenderer sprite;

  // === ���� === //
  private float moveInput;
  private bool isJumpPressed;

  void Awake() {
    // �������� ����������
    rb = GetComponent<Rigidbody2D>();
    sprite = GetComponent<SpriteRenderer>();

    // ��������� ���������
    if (groundCheck == null) {
      Debug.LogError("�� �������� GroundCheck!");
    }
  }

  void Update() {
    // �������� ���� � ����������
    moveInput = Input.GetAxisRaw("Horizontal");

    if (Input.GetButtonDown("Fire1") && canAttack) {
      StartCoroutine(Attack());
    }

    // ������ (��������� ����� ��� ����������� �������� ������)
    if (Input.GetButtonDown("Jump")) {
      if (IsGrounded()) {
        jumpsRemaining = maxJumps; // ����� ��� �����������
      }

      if (jumpsRemaining > 0) {
        isJumpPressed = true;
        jumpsRemaining--;
      }
    }

    // ������� �������
    if (moveInput > 0.01f) {
      sprite.flipX = false;
      isFacingRight = true;
      
    }
    else if (moveInput < -0.01f) {
      sprite.flipX = true;
      isFacingRight = false;
    }

    // ������� �������� ���� � ������� �������
    float targetX = isFacingRight ? 0.7f : -0.7f;
    Vector3 newPos = attackZone.transform.localPosition;
    newPos.x = Mathf.Lerp(newPos.x, targetX, Time.deltaTime * 10f);
    attackZone.transform.localPosition = newPos;

  }

  public void FixedUpdate() {
    // �������� (����� velocity ��� ���������)
    rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocityY);

    // ������ (����� AddForce)
    if (isJumpPressed) {
      rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
      isJumpPressed = false;
    }
  }

  public IEnumerator Attack() {
    canAttack = false;
    attackZone.ToggleAttack(true); // �������� ���� �����

    yield return new WaitForSeconds(attackDuration);

    attackZone.ToggleAttack(false); // ��������� ����
    canAttack = true;
  }

  public bool IsGrounded() {
    // ��������� ���������� ��� ������
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
    // �������������� �������� (��������, ����� ��������)
  }

  // ������������ � ���������
  void OnDrawGizmos() {
    Gizmos.color = Color.yellow;
    Gizmos.DrawWireSphere(
        groundCheck.position,
        checkRadius
    );
  }
}