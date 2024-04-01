using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEngine;

public class Player : MonoBehaviour
{
    // �÷��̾� �̵��ӵ�
    public float speed;
    // ���� �迭
    public GameObject[] weapons;
    // ���� ȹ�� Ȯ�� �迭
    public bool[] hasWeapons;
    // ����ź �迭
    public GameObject[] grenades;
    public int hasGrenades;

    public int ammo;
    public int coin;
    public int health;

    public int maxAmmo;
    public int maxCoin;
    public int maxHealth;
    public int maxHasGrenades;

    // �����¿� �̵�
    float hAxis;
    float vAxis;

    // ���� ����ϰ� �ִ� ����
    int equipWeaponIndex = -1;
    
    // �ȱ�
    bool wDown;
    // ����
    bool jDown;
    // ����
    bool fDown;
    // ȹ��
    bool iDown;
    // ���� ����
    bool swapDown1;
    bool swapDown2;
    bool swapDown3;

    // ���� Ȯ��
    bool isJump;
    // ȸ�� Ȯ��
    bool isDodge;
    // ���� Ȯ��
    bool isSwap;
    // ���� Ȯ��
    bool isFireReady = true;

    // �̵� ����
    Vector3 moveVec;
    // ȸ�� �̵� ����
    Vector3 dodgeVec;

    Rigidbody rb;
    Animator anim;

    // ���� �ִ� ���ӿ�����Ʈ
    GameObject nearObject;
    // ����ϰ� �ִ� ���ӿ�����Ʈ
    Weapon equipWeapon;
    // ���� ������
    float fireDelay;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        GetInput();
        Move();
        Turn();
        Jump();
        Attack();
        Dodge();
        Interation();
        Swap();
    }

    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        wDown = Input.GetButton("Walk");
        jDown = Input.GetButtonDown("Jump");
        fDown = Input.GetButton("Fire1");
        iDown = Input.GetButtonDown("Interation");
        swapDown1 = Input.GetButtonDown("Swap1");
        swapDown2 = Input.GetButtonDown("Swap2");
        swapDown3 = Input.GetButtonDown("Swap3");
    }

    // �̵� �Լ�
    void Move()
    {
        // �̵����͸� 1�� ����ȭ
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;

        if (isDodge) 
            moveVec = dodgeVec;

        if (isSwap || !isFireReady) 
            moveVec = Vector3.zero;

        transform.position += moveVec * speed * (wDown ? 0.3f : 1.0f) * Time.deltaTime;

        anim.SetBool("IsRun", moveVec != Vector3.zero);
        anim.SetBool("IsWalk", wDown);
    }

    void Turn()
    {
        transform.LookAt(transform.position + moveVec);
    }

    void Jump()
    {
        if(jDown && moveVec == Vector3.zero && !isJump && !isDodge && !isSwap)
        {
            rb.AddForce(Vector3.up * 15, ForceMode.Impulse);
            anim.SetBool("IsJump", true);
            anim.SetTrigger("DoJump");
            isJump = true;
        }
    }

    void Attack()
    {
        if (equipWeapon == null)
            return;

        fireDelay += Time.deltaTime;
        isFireReady = equipWeapon.rate < fireDelay;

        if(fDown && isFireReady && !isDodge && !isSwap)
        {
            equipWeapon.Use();
            anim.SetTrigger(equipWeapon.type == Weapon.Type.Melee ? "DoSwing" : "DoShot");
            fireDelay = 0;
        }
    }

    void Dodge()
    {
        if (jDown && moveVec != Vector3.zero && !isJump && !isDodge && !isSwap)
        {
            dodgeVec = moveVec;
            speed *= 2;
            anim.SetTrigger("DoDodge");
            isDodge = true;

            Invoke("DodgeOut", 0.4f);
        }
    }
    void DodgeOut()
    {
        speed *= 0.5f;
        isDodge = false;
    }

    void Swap()
    {
        if (swapDown1 && (!hasWeapons[0] || equipWeaponIndex == 0))
            return;
        if (swapDown2 && (!hasWeapons[1] || equipWeaponIndex == 1))
            return;
        if (swapDown3 && (!hasWeapons[2] || equipWeaponIndex == 2))
            return;

        int weaponIndex = -1;

        if (swapDown1) weaponIndex = 0;
        if (swapDown2) weaponIndex = 1;
        if (swapDown3) weaponIndex = 2;

        if((swapDown1 || swapDown2 || swapDown3) && !isDodge && !isJump)
        {
            if(equipWeapon != null)
                equipWeapon.gameObject.SetActive(false);

            equipWeaponIndex = weaponIndex;
            equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();
            equipWeapon.gameObject.SetActive(true);

            anim.SetTrigger("DoSwap");

            isSwap = true;

            Invoke("SwapOut", 0.4f);
        }
    }

    void SwapOut()
    {
        isSwap = false;
    }

    void Interation()
    {
        if(iDown && nearObject != null && !isDodge && !isJump)
        {
            if(nearObject.tag == "Weapon")
            {
                Item item = nearObject.GetComponent<Item>();
                int weaponIndex = item.value;
                hasWeapons[weaponIndex] = true;

                Destroy(nearObject);
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Floor")
        {
            anim.SetBool("IsJump", false);
            isJump = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Item")
        {
            Item item = other.GetComponent<Item>();
            switch (item.type)
            {
                case Item.Type.Ammo:
                    ammo += item.value;
                    if(ammo > maxAmmo)
                        ammo = maxAmmo;
                    break;
                case Item.Type.Coin:
                    coin += item.value;
                    if (coin > maxCoin)
                        coin = maxCoin;
                    break;
                case Item.Type.Heart:
                    health += item.value;
                    if (health > maxHealth)
                        health = maxHealth;
                    break;
                case Item.Type.Grenade:
                    grenades[hasGrenades].SetActive(true);
                    hasGrenades += item.value;
                    if (hasGrenades > maxHasGrenades)
                        hasGrenades = maxHasGrenades;
                    break;
            }

            Destroy(other.gameObject);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if(other.tag == "Weapon")
            nearObject = other.gameObject;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Weapon")
            nearObject = null;
    }
}
