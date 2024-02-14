using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MonsterGhostCharacterButton : MonoBehaviour
{

    public GameObject Monster;
    public GameObject ShootPoint;
    public SkinnedMeshRenderer bodyMesh;
    public SkinnedMeshRenderer faceMesh;

    public Texture[] faceTextureArray = new Texture[9];
    public Texture[] bodyTextureArray = new Texture[4];
    public GameObject[] effPrefabArray = new GameObject[9];

    public int BodyNum;
    public int HatNum;
    public int SkinNum;

    // Use this for initialization
    void Start()
    {

    }

    void EffectClear()
    {
        GameObject tFindObj = GameObject.FindGameObjectWithTag("Effect");
        if (tFindObj != null)
        {
            DestroyImmediate(tFindObj);
        }
    }


    void OnGUI()
    {
        if (GUI.Button(new Rect(20, 20, 70, 40), "Idle"))
        {
            EffectClear();
            Monster.GetComponent<Animation>().wrapMode = WrapMode.Loop;
            Monster.GetComponent<Animation>().CrossFade("Idle");
            faceMesh.materials[0].SetTexture("_MainTex", faceTextureArray[0]);

            //Vector3 playerV = new Vector3(ShootPoint.transform.position.x, ShootPoint.transform.position.y, ShootPoint.transform.position.z);
            //GameObject tObj = Instantiate(effPrefabArray[8], new Vector3(playerV.x, playerV.y, playerV.z), Monster.transform.rotation);
            //if (GameObject.FindGameObjectWithTag("Effect") == null)
            //{
            //    GameObject tObj = GameObject.Instantiate(effPrefabArray[8]);         
            //    tObj.transform.SetParent(ShootPoint.transform);
            //}
        }
        if (GUI.Button(new Rect(90, 20, 70, 40), "Stand"))
        {
            EffectClear();
            Monster.GetComponent<Animation>().wrapMode = WrapMode.Loop;
            Monster.GetComponent<Animation>().CrossFade("Stand");
            faceMesh.materials[0].SetTexture("_MainTex", faceTextureArray[0]);
            if (GameObject.FindGameObjectWithTag("Effect") == null) GameObject.Instantiate(effPrefabArray[6]);

        }
        if (GUI.Button(new Rect(160, 20, 70, 40), "Move"))
        {
            EffectClear();
            Monster.GetComponent<Animation>().wrapMode = WrapMode.Loop;
            Monster.GetComponent<Animation>().CrossFade("Move");
            faceMesh.materials[0].SetTexture("_MainTex", faceTextureArray[0]);
            if (GameObject.FindGameObjectWithTag("Effect") == null) GameObject.Instantiate(effPrefabArray[5]);
        }
        if (GUI.Button(new Rect(230, 20, 70, 40), "Attack"))
        {
            EffectClear();
            Monster.GetComponent<Animation>().wrapMode = WrapMode.Once;
            Monster.GetComponent<Animation>().CrossFade("Attack");
            faceMesh.materials[0].SetTexture("_MainTex", faceTextureArray[0]);

            Vector3 playerV = new Vector3(ShootPoint.transform.position.x, ShootPoint.transform.position.y, ShootPoint.transform.position.z);
            GameObject tObj = Instantiate(effPrefabArray[0], new Vector3(playerV.x, playerV.y, playerV.z), Monster.transform.rotation);
            tObj.transform.SetParent(ShootPoint.transform);

        }
        if (GUI.Button(new Rect(300, 20, 70, 40), "Skill"))
        {
            EffectClear();
            Monster.GetComponent<Animation>().wrapMode = WrapMode.Once;
            Monster.GetComponent<Animation>().CrossFade("Skill");
            faceMesh.materials[0].SetTexture("_MainTex", faceTextureArray[0]);

            Vector3 playerV = new Vector3(ShootPoint.transform.position.x, ShootPoint.transform.position.y, ShootPoint.transform.position.z);
            GameObject tObj = Instantiate(effPrefabArray[2], new Vector3(playerV.x, playerV.y, playerV.z), Monster.transform.rotation);
            tObj.transform.SetParent(ShootPoint.transform);
        }
        if (GUI.Button(new Rect(370, 20, 70, 40), "Damage"))
        {
            EffectClear();
            Monster.GetComponent<Animation>().wrapMode = WrapMode.Once;
            Monster.GetComponent<Animation>().CrossFade("Damage");
            faceMesh.materials[0].SetTexture("_MainTex", faceTextureArray[0]);
            if (GameObject.FindGameObjectWithTag("Effect") == null) GameObject.Instantiate(effPrefabArray[3]);
        }

        if (GUI.Button(new Rect(440, 20, 70, 40), "Stun"))
        {
            EffectClear();
            Monster.GetComponent<Animation>().wrapMode = WrapMode.Loop;
            Monster.GetComponent<Animation>().CrossFade("Stun");
            faceMesh.materials[0].SetTexture("_MainTex", faceTextureArray[0]);
            if (GameObject.FindGameObjectWithTag("Effect") == null) GameObject.Instantiate(effPrefabArray[9]);
        }

        if (GUI.Button(new Rect(510, 20, 90, 40), "KnockDown"))
        {
            EffectClear();
            Monster.GetComponent<Animation>().wrapMode = WrapMode.Once;
            Monster.GetComponent<Animation>().CrossFade("KnockDown");
            faceMesh.materials[0].SetTexture("_MainTex", faceTextureArray[0]);
            if (GameObject.FindGameObjectWithTag("Effect") == null) GameObject.Instantiate(effPrefabArray[3]);
        }
        if (GUI.Button(new Rect(600, 20, 70, 40), "Sleep"))
        {
            EffectClear();
            Monster.GetComponent<Animation>().wrapMode = WrapMode.Loop;
            Monster.GetComponent<Animation>().CrossFade("Sleep");
            faceMesh.materials[0].SetTexture("_MainTex", faceTextureArray[0]);
            if (GameObject.FindGameObjectWithTag("Effect") == null) GameObject.Instantiate(effPrefabArray[1]);
        }
        if (GUI.Button(new Rect(670, 20, 70, 40), "Awake"))
        {
            EffectClear();
            Monster.GetComponent<Animation>().wrapMode = WrapMode.Once;
            Monster.GetComponent<Animation>().CrossFade("Awake");
            faceMesh.materials[0].SetTexture("_MainTex", faceTextureArray[0]);
            if (GameObject.FindGameObjectWithTag("Effect") == null) GameObject.Instantiate(effPrefabArray[7]);
            
        }
        if (GUI.Button(new Rect(740, 20, 70, 40), "Die"))
        {
            EffectClear();
            Monster.GetComponent<Animation>().wrapMode = WrapMode.Once;
            Monster.GetComponent<Animation>().CrossFade("Die");
            faceMesh.materials[0].SetTexture("_MainTex", faceTextureArray[0]);
            if (GameObject.FindGameObjectWithTag("Effect") == null) GameObject.Instantiate(effPrefabArray[4]);
        }

        ///////////////////////////////////////////////////////////////////////

        if (GUI.Button(new Rect(20, 60, 70, 40), "F-Idle"))
        {
            EffectClear();
            Monster.GetComponent<Animation>().wrapMode = WrapMode.Loop;
            Monster.GetComponent<Animation>().CrossFade("_Idle");
            faceMesh.materials[0].SetTexture("_MainTex", faceTextureArray[6]);

            //Vector3 playerV = new Vector3(ShootPoint.transform.position.x, ShootPoint.transform.position.y, ShootPoint.transform.position.z);
            //GameObject tObj = Instantiate(effPrefabArray[8], new Vector3(playerV.x, playerV.y, playerV.z), Monster.transform.rotation);
            //tObj.transform.SetParent(ShootPoint.transform);
        }
        if (GUI.Button(new Rect(90, 60, 70, 40), "F-Stand"))
        {
            EffectClear();
            Monster.GetComponent<Animation>().wrapMode = WrapMode.Loop;
            Monster.GetComponent<Animation>().CrossFade("_Stand");
            faceMesh.materials[0].SetTexture("_MainTex", faceTextureArray[8]);

            if (GameObject.FindGameObjectWithTag("Effect") == null) GameObject.Instantiate(effPrefabArray[6]);
        }
        if (GUI.Button(new Rect(160, 60, 70, 40), "F-Move"))
        {
            EffectClear();
            Monster.GetComponent<Animation>().wrapMode = WrapMode.Loop;
            Monster.GetComponent<Animation>().CrossFade("_Move");
            faceMesh.materials[0].SetTexture("_MainTex", faceTextureArray[5]);
            if (GameObject.FindGameObjectWithTag("Effect") == null) GameObject.Instantiate(effPrefabArray[5]);
        }
        if (GUI.Button(new Rect(230, 60, 70, 40), "F-Attack"))
        {
            EffectClear();
            Monster.GetComponent<Animation>().wrapMode = WrapMode.Once;
            Monster.GetComponent<Animation>().CrossFade("_Attack");
            faceMesh.materials[0].SetTexture("_MainTex", faceTextureArray[3]);

            Vector3 playerV = new Vector3(ShootPoint.transform.position.x, ShootPoint.transform.position.y, ShootPoint.transform.position.z);
            GameObject tObj = Instantiate(effPrefabArray[0], new Vector3(playerV.x, playerV.y, playerV.z), Monster.transform.rotation);
            tObj.transform.SetParent(ShootPoint.transform);

        }
        if (GUI.Button(new Rect(300, 60, 70, 40), "F-Skill"))
        {
            EffectClear();
            Monster.GetComponent<Animation>().wrapMode = WrapMode.Once;
            Monster.GetComponent<Animation>().CrossFade("_Skill");
            faceMesh.materials[0].SetTexture("_MainTex", faceTextureArray[1]);

            Vector3 playerV = new Vector3(ShootPoint.transform.position.x, ShootPoint.transform.position.y, ShootPoint.transform.position.z);
            GameObject tObj = Instantiate(effPrefabArray[2], new Vector3(playerV.x, playerV.y, playerV.z), Monster.transform.rotation);
            tObj.transform.SetParent(ShootPoint.transform);
        }
        if (GUI.Button(new Rect(370, 60, 70, 40), "F-Damage"))
        {
            EffectClear();
            Monster.GetComponent<Animation>().wrapMode = WrapMode.Once;
            Monster.GetComponent<Animation>().CrossFade("_Damage");
            faceMesh.materials[0].SetTexture("_MainTex", faceTextureArray[2]);
            if (GameObject.FindGameObjectWithTag("Effect") == null) GameObject.Instantiate(effPrefabArray[3]);
        }

        if (GUI.Button(new Rect(440, 60, 70, 40), "F-Stun"))
        {
            EffectClear();
            Monster.GetComponent<Animation>().wrapMode = WrapMode.Loop;
            Monster.GetComponent<Animation>().CrossFade("_Stun");
            faceMesh.materials[0].SetTexture("_MainTex", faceTextureArray[9]);
            if (GameObject.FindGameObjectWithTag("Effect") == null) GameObject.Instantiate(effPrefabArray[9]);
        }

        if (GUI.Button(new Rect(510, 60, 90, 40), "F-KnockDown"))
        {
            EffectClear();
            Monster.GetComponent<Animation>().wrapMode = WrapMode.Once;
            Monster.GetComponent<Animation>().CrossFade("_KnockDown");
            faceMesh.materials[0].SetTexture("_MainTex", faceTextureArray[10]);
            if (GameObject.FindGameObjectWithTag("Effect") == null) GameObject.Instantiate(effPrefabArray[3]);
        }
        if (GUI.Button(new Rect(600, 60, 70, 40), "F-Sleep"))
        {
            EffectClear();
            Monster.GetComponent<Animation>().wrapMode = WrapMode.Loop;
            Monster.GetComponent<Animation>().CrossFade("_Sleep");
            faceMesh.materials[0].SetTexture("_MainTex", faceTextureArray[6]);
            if (GameObject.FindGameObjectWithTag("Effect") == null) GameObject.Instantiate(effPrefabArray[1]);
        }
        if (GUI.Button(new Rect(670, 60, 70, 40), "F-Awake"))
        {
            EffectClear();
            Monster.GetComponent<Animation>().wrapMode = WrapMode.Once;
            Monster.GetComponent<Animation>().CrossFade("_Awake");
            faceMesh.materials[0].SetTexture("_MainTex", faceTextureArray[4]);
            if (GameObject.FindGameObjectWithTag("Effect") == null) GameObject.Instantiate(effPrefabArray[7]);

        }
        if (GUI.Button(new Rect(740, 60, 70, 40), "F-Die"))
        {
            EffectClear();
            Monster.GetComponent<Animation>().wrapMode = WrapMode.Once;
            Monster.GetComponent<Animation>().CrossFade("_Die");
            faceMesh.materials[0].SetTexture("_MainTex", faceTextureArray[10]);
            if (GameObject.FindGameObjectWithTag("Effect") == null) GameObject.Instantiate(effPrefabArray[4]);
        }

        /////////////////////////////////////////////////////////////////////
        if (GUI.Button(new Rect(20, 700, 120, 40), "RandomFace"))
        {
            faceMesh.materials[0].SetTexture("_MainTex", faceTextureArray[Random.Range(0, faceTextureArray.Length)]);
        }
        if (GUI.Button(new Rect(150, 700, 70, 40), "Face01"))
        {
            faceMesh.materials[0].SetTexture("_MainTex", faceTextureArray[1]);
        }
        if (GUI.Button(new Rect(220, 700, 70, 40), "Face02"))
        {
            faceMesh.materials[0].SetTexture("_MainTex", faceTextureArray[2]);
        }
        if (GUI.Button(new Rect(290, 700, 70, 40), "Face03"))
        {
            faceMesh.materials[0].SetTexture("_MainTex", faceTextureArray[3]);
        }
        if (GUI.Button(new Rect(360, 700, 70, 40), "Face04"))
        {
            faceMesh.materials[0].SetTexture("_MainTex", faceTextureArray[4]);
        }
        if (GUI.Button(new Rect(430, 700, 70, 40), "Face05"))
        {
            faceMesh.materials[0].SetTexture("_MainTex", faceTextureArray[5]);
        }
        if (GUI.Button(new Rect(500, 700, 70, 40), "Face06"))
        {
            faceMesh.materials[0].SetTexture("_MainTex", faceTextureArray[6]);
        }
        if (GUI.Button(new Rect(570, 700, 70, 40), "Face07"))
        {
            faceMesh.materials[0].SetTexture("_MainTex", faceTextureArray[7]);
        }
        if (GUI.Button(new Rect(640, 700, 70, 40), "Face08"))
        {
            faceMesh.materials[0].SetTexture("_MainTex", faceTextureArray[8]);
        }
        if (GUI.Button(new Rect(710, 700, 70, 40), "Face09"))
        {
            faceMesh.materials[0].SetTexture("_MainTex", faceTextureArray[9]);
        }
        if (GUI.Button(new Rect(780, 700, 70, 40), "Face10"))
        {
            faceMesh.materials[0].SetTexture("_MainTex", faceTextureArray[10]);
        }

        /////////////////////////////////////////////////////////////////////////////////

        
        if (GUI.Button(new Rect(20, 740, 120, 40), "RandomBody"))
        {
            bodyMesh.materials[0].SetTexture("_MainTex", bodyTextureArray[Random.Range(0, bodyTextureArray.Length)]);
        }
        if (GUI.Button(new Rect(150, 740, 70, 40), "Body_01"))
        {
            BodyNum = 0;
            hSkinNum();
        }
        if (GUI.Button(new Rect(220, 740, 70, 40), "Body_02"))
        {
            BodyNum = 1;
            hSkinNum();
        }
        if (GUI.Button(new Rect(290, 740, 70, 40), "Body_03"))
        {
            BodyNum = 2;
            hSkinNum();
        }
        if (GUI.Button(new Rect(360, 740, 70, 40), "Body_04"))
        {
            BodyNum = 3;
            hSkinNum();
        }
        if (GUI.Button(new Rect(430, 740, 70, 40), "Body_05"))
        {
            BodyNum = 4;
            hSkinNum();
        }
        if (GUI.Button(new Rect(500, 740, 70, 40), "Body_06"))
        {
            BodyNum = 5;
            hSkinNum();
        }
        if (GUI.Button(new Rect(570, 740, 70, 40), "Body_07"))
        {
            BodyNum = 6;
            hSkinNum();
        }
        if (GUI.Button(new Rect(640, 740, 70, 40), "Body_08"))
        {
            BodyNum = 7;
            hSkinNum();
        }
        if (GUI.Button(new Rect(710, 740, 70, 40), "Body_09"))
        {
            BodyNum = 8;
            hSkinNum();
        }
        if (GUI.Button(new Rect(780, 740, 70, 40), "Body_10"))
        {
            BodyNum = 9;
            hSkinNum();
        }
        ////////////////////////////////////////////////////////////////////
        if (GUI.Button(new Rect(150, 780, 70, 40), "Hat_01"))
        {
            HatNum = 0;
            hSkinNum();
        }
        if (GUI.Button(new Rect(220, 780, 70, 40), "Hat_02"))
        {
            HatNum = 1;
            hSkinNum();
        }
        if (GUI.Button(new Rect(290, 780, 70, 40), "Hat_03"))
        {
            HatNum = 2;
            hSkinNum();
        }
        if (GUI.Button(new Rect(360, 780, 70, 40), "Hat_04"))
        {
            HatNum = 3;
            hSkinNum();
        }
        if (GUI.Button(new Rect(430, 780, 70, 40), "Hat_05"))
        {
            HatNum = 4;
            hSkinNum();
        }
        if (GUI.Button(new Rect(500, 780, 70, 40), "Hat_06"))
        {
            HatNum = 5;
            hSkinNum();
        }
        if (GUI.Button(new Rect(570, 780, 70, 40), "Hat_07"))
        {
            HatNum = 6;
            hSkinNum();
        }
        if (GUI.Button(new Rect(640, 780, 70, 40), "Hat_08"))
        {
            HatNum = 7;
            hSkinNum();
        }
        if (GUI.Button(new Rect(710, 780, 70, 40), "Hat_09"))
        {
            HatNum = 8;
            hSkinNum();
        }
        if (GUI.Button(new Rect(780, 780, 70, 40), "Hat_10"))
        {
            HatNum = 9;
            hSkinNum();
        }
        ////////////////////////////////////////////////////////////////////

        if (GUI.Button(new Rect(720, 420, 120, 40), "Ghost01"))
        {
            SceneManager.LoadScene("Ghost_01");
        }

        if (GUI.Button(new Rect(720, 460, 120, 40), "Ghost02"))
        {
            SceneManager.LoadScene("Ghost_02");
        }

        if (GUI.Button(new Rect(720, 500, 120, 40), "Ghost03"))
        {
            SceneManager.LoadScene("Ghost_03");
        }
        if (GUI.Button(new Rect(720, 540, 120, 40), "Ghost04"))
        {
            SceneManager.LoadScene("Ghost_04");
        }
    }

    void hSkinNum()
    {
        SkinNum = BodyNum * 10 + HatNum;
        bodyMesh.materials[0].SetTexture("_MainTex", bodyTextureArray[SkinNum]);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
    }

}
