using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEngine;

public class Player : MonoBehaviour
{
    // 플레이어 이동속도
    public float speed;
    // 무기 배열
    public GameObject[] weapons;
    // 무기 획득 확인 배열
    public bool[] hasWeapons;

    // 상하좌우 이동
    float hAxis;
    float vAxis;

    // 현재 장비하고 있는 무기
    int equipWeaponIndex = -1;
    
    // 걷기
    bool wDown;
    // 점프
    bool jDown;
    // 획득
    bool iDown;
    // 무기 스왑
    bool swapDown1;
    bool swapDown2;
    bool swapDown3;

    // 점프 확인
    bool isJump;
    // 회피 확인
    bool isDodge;
    // 스왑 확인
    bool isSwap;

    // 이동 벡터
    Vector3 moveVec;
    // 회피 이동 벡터
    Vector3 dodgeVec;

    Rigidbody rb;
    Animator anim;

    // 옆에 있는 게임오브젝트
    GameObject nearObject;
    // 장비하고 있는 게임오브젝트
    GameObject equipWeapon;

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
        iDown = Input.GetButtonDown("Interation");
        swapDown1 = Input.GetButtonDown("Swap1");
        swapDown2 = Input.GetButtonDown("Swap2");
        swapDown3 = Input.GetButtonDown("Swap3");
    }

    // 이동 함수
    void Move()
    {
        // 이동벡터를 1로 정규화
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;

        if (isDodge) moveVec = dodgeVec;

        if (isSwap) moveVec = Vector3.zero;
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
                equipWeapon.SetActive(false);

            equipWeaponIndex = weaponIndex;
            equipWeapon = weapons[weaponIndex];
            equipWeapon.SetActive(true);

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
