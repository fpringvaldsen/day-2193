﻿using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

  public float playerSpeed;
  public float health;
  public float maxHealth;
  public float invincibilityWindow = 1.0f;

  public GameObject projectile;
  private float fireDelay = 0f;

  private Rigidbody2D rb;
  private GameManager gameManager;
  private bool invincible;
  private bool canInteract;
  private float healthRestorationRate;

  private GameObject collidingStation;
  private SpriteRenderer sr;

  public Vector2 playerDirection;

  // Use this for initialization
  void Start () {
    playerDirection = new Vector2(0f, 2f);
    rb = GetComponent<Rigidbody2D>();
    gameManager = GameObject.FindGameObjectWithTag(Tags.GAME_CONTROLLER).GetComponent<GameManager>();
    maxHealth = 100f;
    health = maxHealth;
    invincible = false;
    canInteract = false;
    healthRestorationRate = 5.0f;
    sr = gameObject.GetComponent<SpriteRenderer>();
  }

  public Vector2 getPlayerDirection() {
    return this.playerDirection;
  }

  void Attack() {
    Vector3 playerPosition = transform.position;

    Debug.Log(playerDirection);
    if (playerDirection.x == 0 && playerDirection.y > 0) {
      playerPosition.y += 1.0f;
    } else if (playerDirection.x == 0 && playerDirection.y < 0) {
      playerPosition.y -= 1.0f;
    } else if (playerDirection.x > 0 && playerDirection.y == 0) {
      playerPosition.x += 1.0f;
    } else if (playerDirection.x < 0 && playerDirection.y == 0) {
      playerPosition.x -= 1.0f;
    }

    Instantiate(projectile, playerPosition, Quaternion.identity);

  }

  // Update is called once per frame
  void Update () {
    if (fireDelay > 0f) {
      fireDelay -= Time.deltaTime;
    }

    if (Input.GetKey(KeyCode.LeftShift) && fireDelay <= 0) {
      Attack();
      fireDelay = 0.3f;
    }

    Move();
    InteractWithStation();
  }

  void OnTriggerEnter2D(Collider2D other) {
    if (other.gameObject.tag == Tags.RESTORATION_STATION) {
      collidingStation = other.gameObject;
      canInteract = true;
    }
  }

  void OnTriggerExit2D(Collider2D other) {
    if (other.gameObject.tag == Tags.RESTORATION_STATION) {
      canInteract = false;
    } 
  }

  void Move() {
    float xVel = Input.GetAxisRaw("Horizontal") * playerSpeed;
    float yVel = Input.GetAxisRaw("Vertical") * playerSpeed;

    if (!(xVel == 0 && yVel == 0)) {
      playerDirection.x = xVel;
      playerDirection.y = yVel;
    }

    if (playerDirection.x > 0) {
      sr.flipX = true;
    } else if (playerDirection.x < 0) {
      sr.flipX = false;
    }

    rb.velocity = new Vector2(xVel, yVel);
  }

  void InteractWithStation() {
    if (canInteract && Input.GetKey(KeyCode.Space)) {
      RestorationStationController rsc = collidingStation.GetComponent<RestorationStationController>();
      if (rsc.name == RestorationStations.HEALTH_STATION) {
        RestoreHealth(healthRestorationRate * Time.deltaTime);
      } else {
        rsc.RestoreResource();
      }
    }
  }

  public void Damage(float damage) {
    if (!invincible) {
      health -= damage;
      if (health <= 0) {
        gameManager.GameOver();
      } else {
        invincible = true;
        StartCoroutine("InvincibilityTimer");
      }
    }
  }

  IEnumerator InvincibilityTimer() {
    yield return new WaitForSeconds(invincibilityWindow);
    invincible = false;
  }

  public void RestoreHealth(float toRestore) {
    health = Mathf.Min(health + toRestore, maxHealth);
  }
}
