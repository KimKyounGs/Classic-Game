using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Player : MonoBehaviour
{
    public bool isTouchTop;
    public bool isTouchBottom;
    public bool isTouchRight;
    public bool isTouchLeft;

    public int life;
    public int score;
    public float speed;
    public int power;
    public int maxPower;
    public int boom;
    public int maxBoom;
    public float maxShotDelay; // 최대
    public float curShotDelay; // 현재
    public bool isHit;
    public bool isBoomTime;


    public GameObject bulletObjA;
    public GameObject bulletObjB;
    public GameObject boomEffect;
    public GameManager manager;
    Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();     
    }

    void Update()
    {
        Move();
        Fire();
        Boom();
        Reload();
    }

    void Move()
    {
        float h = Input.GetAxisRaw("Horizontal");
        if ((isTouchRight && h == 1) || (isTouchLeft && h == -1))
        {
            h = 0;
        }
        float v = Input.GetAxisRaw("Vertical");
        if ((isTouchTop && v == 1) || (isTouchBottom && v == -1))
        {
            v = 0;
        }
        Vector3 curPos = transform.position;
        Vector3 nextPos = new Vector3(h, v, 0) * speed * Time.deltaTime;

        transform.position = curPos + nextPos;

        if (Input.GetButtonDown("Horizontal") || Input.GetButtonUp("Horizontal"))
        {
            anim.SetInteger("Input", (int)h);
        }
    }

    void Fire()
    {
        if (!Input.GetButton("Fire1")) return;

        if (curShotDelay < maxShotDelay) return;

        switch(power)
        {
            case 1:
                {
                    GameObject bullet = Instantiate(bulletObjA, transform.position, transform.rotation);
                    Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
                    rigid.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
                    break;
                }
            case 2:
                {
                    GameObject bulletR = Instantiate(bulletObjA, transform.position + Vector3.right * 0.1f, transform.rotation);
                    GameObject bulletL = Instantiate(bulletObjA, transform.position + Vector3.left * 0.1f, transform.rotation);
                    Rigidbody2D rigidR = bulletR.GetComponent<Rigidbody2D>();
                    Rigidbody2D rigidL = bulletL.GetComponent<Rigidbody2D>();
                    rigidR.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
                    rigidL.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
                    break;
                }
            case 3:
                {
                    GameObject bulletRR = Instantiate(bulletObjA, transform.position + Vector3.right * 0.35f, transform.rotation);
                    GameObject bulletCC = Instantiate(bulletObjB, transform.position, transform.rotation);
                    GameObject bulletLL = Instantiate(bulletObjA, transform.position + Vector3.left * 0.35f, transform.rotation);
                    Rigidbody2D rigidRR = bulletRR.GetComponent<Rigidbody2D>();
                    Rigidbody2D rigidCC = bulletCC.GetComponent<Rigidbody2D>();
                    Rigidbody2D rigidLL = bulletLL.GetComponent<Rigidbody2D>();
                    rigidRR.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
                    rigidCC.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
                    rigidLL.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
                    break;
                }
        }

        curShotDelay = 0;
    }

    void Reload()
    {
        curShotDelay += Time.deltaTime;
    }

    void Boom()
    {
        if (!Input.GetButton("Fire2")) return;

        if (isBoomTime) return;

        if (boom == 0) return;

        boom--;
        isBoomTime = true;
        manager.UpdateBoomIcon(boom);

        // 1. Effect visible
        boomEffect.SetActive(true);
        Invoke("OffBoomEffect", 4f);

        // 2. Remove Enemy
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        for (int index = 0; index < enemies.Length; index++)
        {
            Enemy enemyLogic = enemies[index].GetComponent<Enemy>();
            enemyLogic.OnHit(1000);
        }

        // 3. Remove EnemyBullet
        GameObject[] bullets = GameObject.FindGameObjectsWithTag("EnemyBullet");
        for (int index = 0; index < bullets.Length; index++)
        {
            Destroy(bullets[index]);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Border")
        {
            switch (collision.gameObject.name)
            {
                case "Top":
                    isTouchTop = true;
                    break;
                case "Bottom":
                    isTouchBottom = true;
                    break;
                case "Right":
                    isTouchRight = true;
                    break;
                case "Left":
                    isTouchLeft = true;
                    break;
            }
        }
        else if (collision.gameObject.tag == "Enemy" || collision.gameObject.tag == "EnemyBullet")
        {
            if (isHit) return;

            isHit = true;
            life--;
            manager.UpdateLifeIcon(life);

            if (life == 0)
            {
                manager.GameOver();
            }
            else
            {
                manager.RespawnPlayer();
            }
            gameObject.SetActive(false);
            Destroy(collision.gameObject);
        }
        else if (collision.gameObject.tag == "Item")
        {
            Item item = collision.gameObject.GetComponent<Item>();
            switch (item.type)
            {
                case "Coin":
                    score += 1000;
                    break;
                case "Power":
                    if (maxPower == power)
                    {
                        score += 500;
                    }
                    else
                    {
                        power++;
                    }
                    break;
                case "Boom":
                    if (maxBoom == boom)
                    {
                        score += 500;
                    }
                    else
                    {
                        boom++;
                        manager.UpdateBoomIcon(boom);
                    }
                    break;
            }
            Destroy(collision.gameObject);
        }
    }

    void OffBoomEffect()
    {
        boomEffect.SetActive(false);
        isBoomTime = false;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Border")
        {
            switch (collision.gameObject.name)
            {
                case "Top":
                    isTouchTop = false;
                    break;
                case "Bottom":
                    isTouchBottom = false;
                    break;
                case "Right":
                    isTouchRight = false;
                    break;
                case "Left":
                    isTouchLeft = false;
                    break;
            }
        }
    }
}
