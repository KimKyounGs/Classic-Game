using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public string enemyName;
    public float speed;
    public int health;
    public int enemyScore;
    public Sprite[] sprites;
  
    public float maxShotDelay; // 최대
    public float curShotDelay; // 현재

    public GameObject bulletObjA;
    public GameObject bulletObjB;
    public GameObject player;

    public GameObject itemCoin;
    public GameObject itemPower;
    public GameObject itemBoom;

    SpriteRenderer spriteRenderer;
    Rigidbody2D rigid;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rigid = GetComponent<Rigidbody2D>();
        rigid.velocity = Vector2.down * speed;
    }
    void Update()
    {
        Fire();
        Reload();
    }
    void Fire()
    {
        if (curShotDelay < maxShotDelay) return;

        if (enemyName == "S")
        {
            GameObject bullet = Instantiate(bulletObjA, transform.position, transform.rotation);
            Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
            Vector3 dirVec = player.transform.position - transform.position;
            rigid.AddForce(dirVec.normalized * 5, ForceMode2D.Impulse);   
        }
        else if (enemyName == "L")
        {
            GameObject bulletR = Instantiate(bulletObjB, transform.position + Vector3.right*0.3f, transform.rotation);
            GameObject bulletL = Instantiate(bulletObjB, transform.position + Vector3.left*0.3f, transform.rotation);
            Rigidbody2D rigidR = bulletR.GetComponent<Rigidbody2D>();
            Rigidbody2D rigidL = bulletL.GetComponent<Rigidbody2D>();
            Vector3 dirVecR = player.transform.position - (transform.position + Vector3.right * 0.3f);
            Vector3 dirVecL = player.transform.position - (transform.position + Vector3.left * 0.3f);
            rigidR.AddForce(dirVecR.normalized * 5, ForceMode2D.Impulse);
            rigidL.AddForce(dirVecL.normalized * 5, ForceMode2D.Impulse);
        }

        curShotDelay = 0;
    }

    void Reload()
    {
        curShotDelay += Time.deltaTime;
    }

    public void OnHit(int dmg)
    {
        if (health <= 0) return;

        health -= dmg;
        spriteRenderer.sprite = sprites[1];
        Invoke("ReturnSprite", 0.1f);

        if (health <= 0)
        {
            Player playerLogic = player.GetComponent<Player>();
            playerLogic.score += enemyScore;

            int ran = Random.Range(0, 10);
            if (ran < 3) // No Item 30%
            {
                Debug.Log("No Item");
            }
            else if (ran < 6)  // Coin 30%
            {
                Instantiate(itemCoin, transform.position, itemCoin.transform.rotation);
            }
            else if (ran < 8) // Power 20%
            {
                Instantiate(itemPower, transform.position, itemCoin.transform.rotation);
            }
            else if (ran < 10) // Boom 20%
            {
                Instantiate(itemBoom, transform.position, itemCoin.transform.rotation);
            }
            Destroy(gameObject);
        }
    }

    void ReturnSprite()
    {
        spriteRenderer.sprite = sprites[0];
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "BorderBullet")
        {
            Destroy(gameObject);
        }
        else if (collision.gameObject.tag == "PlayerBullet")
        {
            Bullet bullet = collision.gameObject.GetComponent<Bullet>();
            OnHit(bullet.dmg);
            Destroy(collision.gameObject);
        }
    }
}
