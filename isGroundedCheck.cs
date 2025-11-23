using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsGroundedCheck : MonoBehaviour
{
    bool isGrounded;
    bool isOutOfArea;
    bool isGoBack;




    void Awake()
    {
       isGrounded = true;
        isOutOfArea = false;
        isGoBack = true;

        //IsGroundedCheck isGroundedCheck = new IsGroundedCheck();
    }

    public bool IsGrounded()
    {
        return isGrounded;
    }
    public bool IsOutOfArea()
    {
        return isOutOfArea;
    }
    public bool IsGoBack()
    {
       // Debug.Log($"isGoBack:{isGoBack}");

        return isGoBack;
    }


    /*
     * isTriggerがオンなら衝突判定なし・検知のみ
     *isTriggerがオンならOnCollisionEnter 引数Collider other
     *isTriggerがオフならOnTriggerEnter　引数Collision collision
     */

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Ground"))
            isGrounded = false;
        else if (other.gameObject.CompareTag("Center"))
        {
            isGoBack = true;
            Debug.Log("中央にいません");
        }
        /*
        if (collision.gameObject.layer == LayerMask.NameToLayer("OutOfArea"))
            isOutOfArea = false;
        */
    }

    void OnTriggerStay(Collider other)
    {


        if (other.gameObject.CompareTag("Center"))
        {
            //Debug.Log("中央にいます");
            isGoBack = false;
        }

            
        
        //if (other.gameObject.CompareTag("Ground"))
            //Debug.Log("場内にいます");

        
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ground"))
            isGrounded = true;
        else if (other.gameObject.CompareTag("Center"))
        {
            isGoBack = false;//中央のコライダーに入ったら、goBack状態を解除する
            //Debug.Log("中央にいます");
        }

        /*
        if(collision.gameObject.layer == LayerMask.NameToLayer("OutOfArea"))
            isOutOfArea = true;
        */
    }
    


}
